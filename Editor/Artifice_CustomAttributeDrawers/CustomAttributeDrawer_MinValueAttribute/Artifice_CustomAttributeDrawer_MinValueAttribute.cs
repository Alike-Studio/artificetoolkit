using System.Linq;
using ArtificeToolkit.Attributes;
using ArtificeToolkit.Editor.Artifice_CustomAttributeDrawers.CustomAttributeDrawer_Validators;
using ArtificeToolkit.Editor.Resources;
using UnityEditor;
using UnityEngine;

namespace ArtificeToolkit.Editor.Artifice_CustomAttributeDrawers.CustomAttributeDrawer_MinValueAttribute
{
    [Artifice_CustomAttributeDrawer(typeof(MinValueAttribute))]
    public class Artifice_CustomAttributeDrawer_MinValueAttribute : Artifice_CustomAttributeDrawer_Validator_BASE
    {
        public override string LogMessage { get; } = "Property value is below minimum accepted value.";
        public override Sprite LogSprite { get; } = Artifice_SCR_CommonResourcesHolder.instance.ErrorIcon;
        public override LogType LogType { get; } = LogType.Error;
        
        protected override bool IsApplicableToProperty(SerializedProperty property)
        {
            return property.propertyType is SerializedPropertyType.Integer or SerializedPropertyType.Float;
        }

        public override bool IsValid(SerializedProperty property)
        {
            // This can be applied either from the property, or be injected to the property through an array parent. Check both.
            var attribute = (MinValueAttribute)property.GetCustomAttributes().FirstOrDefault(attribute => attribute is MinValueAttribute);
            if (attribute == null)
            {
                attribute = (MinValueAttribute)property.FindParentProperty().GetCustomAttributes().FirstOrDefault(parentAttribute => parentAttribute is MinValueAttribute);
                Debug.Assert(attribute != null , "Cannot find where the property was injected from.");
            }
            
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue >= attribute.Value;
                case SerializedPropertyType.Float:
                    return property.floatValue >= attribute.Value;
                default:
                    return false;
            }
        }
    }
}
