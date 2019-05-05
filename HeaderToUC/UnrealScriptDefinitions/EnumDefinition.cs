﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeaderToUS.UnrealScriptDefinitions
{
    public class EnumDefinition : Definition
    {
        /// <summary>Where to find the definition of this Enum.</summary>
        private const int definitionIndex = 0;
        /// <summary>Where to find the specific details inside the definition.</summary>
        private const int detailsIndex = 2;
        /// <summary>Where to find the package name inside the details.</summary>
        private const int packageNameIndex = 0;
        /// <summary>Where to find the class name inside the details.</summary>
        private const int classNameIndex = 1;
        /// <summary>Where to find the enum name inside the details.</summary>
        private const int enumNameindex = 2;
        /// <summary>Where to find the variables of this Enum in the header definition after it is split.</summary>
        private const int variableBeginIndex = 1;

        private List<string> EnumProperties { get; set; }

        /// <summary>
        /// Gets the details of the <c>Enum</c> from the header definition provided.
        /// </summary>
        /// <param name="headerDefinition">String representation of the header definition file.</param>
        public EnumDefinition(string headerDefinition)
        {
            List<string> lines = headerDefinition.Split('\n').ToList<string>();
            string[] enumDetails = lines[definitionIndex].Split(' ')[detailsIndex].Split('.');

            // Get the enum details from the header file.
            this.PackageName = enumDetails[packageNameIndex];
            this.ClassFileName = enumDetails[classNameIndex];
            this.Name = enumDetails[enumNameindex];

            // Get the lines that are variables.
            string propertyDefinitions = headerDefinition.Split(new Char[] { '{', '}' })[1];
            string[] propertyLines = propertyDefinitions.Split('\n');

            // Create the enum enties array.
            this.EnumProperties = new List<string>();

            // Parse each enum line which are explicitly defined with the value they equal (e.g. `val = 0, valTwo = 1`)
            foreach (string property in propertyLines)
            {
                // Don't parse properties that have no content.
                if(property == "")
                {
                    continue;
                }

                string variableName = property.Split('=')[0];

                // Remove the trailing space if it exists.
                if (variableName[variableName.Length - 1] == ' ')
                {
                    variableName = variableName.Remove(variableName.Length - 1);
                }

                // Add the property.
                EnumProperties.Add(variableName);
            }
        }

        /// <summary>
        /// Modifies the <c>ToString</c> method to return an UnrealScript definition of an <c>Enum</c>.
        /// </summary>
        /// <returns>A valid UnrealScript enum definition.</returns>
        public override string ToString()
        {
            // Define the enum.
            string enumDefinition = "enum " + this.Name + '\n';
            enumDefinition += "{" + '\n';

            // Add the enum properties
            foreach (string property in this.EnumProperties)
            {
                enumDefinition += "    " + property.ToString() + "," + '\n';
            }

            // Add closing brace.
            enumDefinition += "};" + '\n' + '\n';

            return enumDefinition;
        }
    }
}
