using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Flexy.Core.Editor
{
	[CustomPropertyDrawer(typeof(PlymorphAttribute))]
    public class PolymorphPropertyDrawer : PropertyDrawer
    {
        private const String NullName = "None";
		
		private TextField		_prop;
		private VisualElement	_propContainer;

        public override VisualElement CreatePropertyGUI( SerializedProperty property )
		{
			return CreatePropertyGUI( property, property.displayName );
		}
		public VisualElement CreatePropertyGUI( SerializedProperty property, String displayName )
        {
			if ( property.propertyType != SerializedPropertyType.ManagedReference )
				return new PropertyField( property );
				
            var root = new VisualElement( );
			
			var propField	= new TextField( displayName );
			var button		= new Button( ){ text = "⦿" };
			var propsVe		= new VisualElement(  ){ name = "PropsContainer", style = { marginLeft = 13}};
	
			_propContainer	= propsVe;
			_prop			= propField;
			_prop.userData	= property;
			
			propField.AddToClassList(TextField.alignedFieldUssClassName);
			propField.isReadOnly = true;
			
			propField.Add( button );
			root.Add( propField );
			root.Add( propsVe );
			
			var attr				= ((PlymorphAttribute)attribute);
			var baseType			= attr?.BaseType ?? GetType( property.managedReferenceFieldTypename );
			
			//_prop.value				= property.managedReferenceFullTypename;
			
			SetupChooseItemButton( _prop, button, baseType, type => WriteNewInstanceByIndexType(type, property) );
			
			PopulateInnerProps( property );
			
			return root;
		}
		
        public static	void		SetupChooseItemButton( VisualElement root, Button btn, Type propertyType, Action<Type> onSelectedNewType )
        {
	        var assignableTypes = (List<Type>)btn.userData;
	        
	        if( assignableTypes == null )
			{
				assignableTypes = GetAssignableTypes(propertyType);
				btn.userData = assignableTypes;
			}
	        
	        btn.clickable.clicked += ShowDropdown;

	        void ShowDropdown()
	        {
		        var dropdown = new FlexyAdvancedDropdown( assignableTypes.Select(NicifyTypeFullName), index => onSelectedNewType( assignableTypes[index] ) );
		        var buttonMatrix = root.worldTransform;
		        var position = new Vector3(buttonMatrix.m03, buttonMatrix.m13, buttonMatrix.m23);
		        var size = root.contentRect.size;
		        size.x = Math.Max( 400, size.x ); 
		        var buttonRect = new Rect(position, size );
		        
		        dropdown.Show(buttonRect);
	        }
			
			
        }
		
		private static Type			GetType				( String typename )		
		{
			if( String.IsNullOrWhiteSpace( typename ) )
				return null;
			
			var parts		= typename.Split( ' ' );
			return Type.GetType( $"{parts[1]}, {parts[0]}", false );
		}
		private static String		NicifyTypeFullName	( Type type )			=> type == null ? NullName : ObjectNames.NicifyVariableName( type.FullName );
		private static String		NicifyTypeName		( Type type )			=> type == null ? NullName : ObjectNames.NicifyVariableName( type.Name );
        private static List<Type>	GetAssignableTypes	( Type type )			
        {
            var nonUnityTypes	= TypeCache.GetTypesDerivedFrom(type).Where(IsAssignableNonUnityType).ToList();
            nonUnityTypes.Sort( (l, r) => String.Compare( l.FullName, r.FullName, StringComparison.Ordinal) );
            nonUnityTypes.Insert(0, null);
            return nonUnityTypes;

            Boolean IsAssignableNonUnityType(Type type)
            {
                return ( type.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface ) && !type.IsSubclassOf(typeof(UnityEngine.Object)) && type.GetCustomAttributes().All( a => !a.GetType().Name.Contains( "BakingType" )  );
            }
        }

        private void	WriteNewInstanceByIndexType		( Type newType, SerializedProperty property )		
        {
            var newObject = newType != null ? FormatterServices.GetUninitializedObject(newType) : null;
            ApplyValueToProperty(newObject, property);
			
			PopulateInnerProps( (SerializedProperty)_prop.userData );
        }
        private void	ApplyValueToProperty			( Object value, SerializedProperty property )		
        {
            property.managedReferenceValue = value;
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
        }
		private void	PopulateInnerProps				( SerializedProperty property )						
		{
			_propContainer.hierarchy.Clear( );
			
			if( property.managedReferenceValue == null )
			{
				_prop.value = "None";
				return;
			}
			
			var copy	= property.Copy( );
			
			if( copy.NextVisible( true ) )
			{
				var depth = copy.depth;
				do { _propContainer.Add( new PropertyField(copy) ); }
				while ( copy.NextVisible( false ) && copy.depth >= depth );
			}
			
			var selectedType		= GetType( property.managedReferenceFullTypename );
			var selectedTypeName	= NicifyTypeName(selectedType);
			_prop.value				= selectedTypeName;
			
			_propContainer.Unbind( );
			_propContainer.Bind( property.serializedObject );
		}
    }
}
