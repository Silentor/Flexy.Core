﻿using System;
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
		public override Boolean CanCacheInspectorGUI(SerializedProperty property)
		{
			return false;
		}
		
		private const	String			NullName = "None";
		
		private			ContainerField	_propField;
		private			String			_displayName;
		private			VisualElement	_propsInline;
		private			VisualElement	_propsBlock;

        public override	VisualElement	CreatePropertyGUI	( SerializedProperty property )						
		{
			return CreatePropertyGUI( property, property.displayName );
		}
		public			VisualElement	CreatePropertyGUI	( SerializedProperty property, String displayName )	
        {
			if ( property.propertyType != SerializedPropertyType.ManagedReference )
				return new PropertyField( property );
			
			var attr		= ((PlymorphAttribute)attribute);
            var root		= new VisualElement( );
			
			var header		= new VisualElement { name = "Header", style = { flexDirection = FlexDirection.Row }};
			var foldout		= new Foldout { text = " ", style = { width = 0 }};
			var propField	= new ContainerField( displayName ){ style = { flexGrow = 1, flexShrink = 1 } };
			var propsBlock	= new VisualElement { name = "PropsBlock", style = { marginLeft = 13}};
			
			header.Add( foldout );
			header.Add( propField );
			
			var button		= new Button		{ text = "⦿", style = { maxWidth = 20}};
			var propsInline	= new VisualElement { name = "PropsInline", style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };
			
			propField.AddToClassList(ContainerField.alignedFieldUssClassName);
			propField.Add( propsInline );
			propField.Add( new(){ style = { flexGrow = 0.01f } } );
			propField.Add( button );
			
			_propsInline			= propsInline; 
			_displayName			= displayName;
			_propField				= propField;
			_propField.userData		= property;
			_propsBlock				= propsBlock;
	
			SetFieldName( _propField );
			
			foldout.RegisterValueChangedCallback( e => propsBlock.style.display = e.newValue ? DisplayStyle.Flex : DisplayStyle.None );
			
			var baseType			= attr?.BaseType ?? GetType( property.managedReferenceFieldTypename );
			
			SetupChooseItemButton( _propField, button, baseType, type => WriteNewInstanceByIndexType(type, property) );
			
			PopulateInnerProps( property );
			
			if( propsBlock.hierarchy.childCount == 0 )
				foldout.style.display = DisplayStyle.None;
			
			root.Add( header );
			root.Add( _propsBlock );
			
			return root;
		}
		
		private void SetFieldName( ContainerField field )
		{
			var property = (SerializedProperty)field.userData;
			var path = property.propertyPath;
			
			var selectedType		= GetType( property.managedReferenceFullTypename );
			var selectedTypeName	= NicifyTypeName(selectedType);
			
			if( path[^1] == ']' )
			{
				var startIndex	= path.LastIndexOf('[');
				var index		= Int32.Parse( path[(startIndex+1)..^1] );
				
				field.label =  $"{index:D2}.{selectedTypeName}";
			}
			else
			{
				field.label = _displayName + " => " + selectedTypeName;
			}
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
			
			PopulateInnerProps( (SerializedProperty)_propField.userData );
        }
        private void	ApplyValueToProperty			( Object value, SerializedProperty property )		
        {
            property.managedReferenceValue = value;
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
        }
		private void	PopulateInnerProps				( SerializedProperty property )						
		{
			_propsBlock.hierarchy.Clear( );
			
			if( property.managedReferenceValue == null )
			{
				SetFieldName( _propField );
				return;
			}
			
			var attr			= property.managedReferenceValue.GetType().GetCustomAttribute<InlineFieldsAttribute>();
			var inlineFields	= attr != null ? attr.FieldNames : Array.Empty<String>( );
			
			var copy	= property.Copy( );
			
			if( copy.NextVisible( true ) )
			{
				var depth	= copy.depth;
				var i		= 0;
				do 
				{
					var putInline = !copy.isArray && ( i < 4 || inlineFields.Contains( copy.name ) );
					if( putInline )
						_propsInline.Add( new PropertyField(copy, String.Empty) );//{ style = { flexGrow = 1f} } );
					
					else
						_propsBlock.Add( new PropertyField(copy) );
					
					i++;
				}
				while ( copy.NextVisible( false ) && copy.depth >= depth );
			}
			
			SetFieldName( _propField );
			
			_propsBlock.Unbind( );
			_propsBlock.Bind( property.serializedObject );
		}
    }
	
	public class ContainerField : BaseField<ContainerField.voidd>
	{
		public ContainerField( String label ) : base( label, new (){ style = { flexGrow = 0 } } ) 
		{
		
		}
		
		public struct voidd{}
	}
}
