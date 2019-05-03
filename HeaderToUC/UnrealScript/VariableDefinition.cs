﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HeaderToUS.UnrealScriptDefinitions
{
    public class VariableDefinition
    {
        /// <summary>
        /// Enumerable representations of a modifier to be applied to a variable.
        /// </summary>
        public enum VariableModifier
        {
            Edit = 0,
            Const,
            EditConst,
            EditConstArray,
            EditInline,
            EditInlineNotify,
            Localized,
            Export,
            Transient,
            Native,
            Net,
            NoExport
        }

        /// <summary>Name of the variable.</summary>
        private string Name { get; set; }
        /// <summary>Type that this variable will hold.</summary>
        private string Type { get; set; }
        /// <summary>Modifiers to apply to the variable.</summary>
        public List<VariableModifier> Modifiers { get; private set; }

        /// <summary>
        /// Creates a new variable by parsing the header definition provided.
        /// </summary>
        /// <param name="headerDefinition">The definition of this variable provided by the header.</param>
        public VariableDefinition(string headerDefinition)
        {
            // Remove unneeded parts of the definition.
            string cleanedDefinition = headerDefinition.Replace("struct", "").Replace("class", "").Replace("unsigned", "").Replace("1", "").Replace(";", "").Replace(":", "").Replace("<", "").Replace(">", "").Replace("*", "");

            string type = GetVariableType(cleanedDefinition);
            string name = GetVariableName(cleanedDefinition);
            // Set the type of this variable if it's valid.
            if (type != null && name != null)
            {
                this.Type = type;
                this.Name = name;

                // Set the modifiers of this variable.
                SetModifiers(cleanedDefinition);
            }
            else
            {
                // Can't create the variable.
                throw new InvalidVariableException();
            }
        }
        
        /// <summary>
        /// Sets what type of variable this variable is.
        /// Could be a primitive type or a class type, or an array. 
        /// </summary>
        /// <param name="headerDefinition"></param>
        private string GetVariableType(string headerDefinition)
        {
            List<string> splitDefinition = headerDefinition.Split(new Char[] { ' ' }).ToList<string>();
            splitDefinition.RemoveAll(item => item == "");

            // Get the base type of the variable.
            string type = GetType(splitDefinition[0]);
            switch(type)
            {
                case "array":
                    
                    // Append the array type inside here.
                    type += "<" + GetType(splitDefinition[1]) + ">";
                    break;
                case "byte":

                    // Only add if it's not unknown. (Unknown types become 'unsigned char' variables with named 'UnknownData<id>').
                    if (splitDefinition[1].Contains("UnknownData"))
                    {
                        type = null;
                    }
                    break;
            }

            // Return the formatted type.
            return type;
        }


        /// <summary>
        /// Gets the correct type for the type string given.
        /// </summary>
        /// <param name="type">Header representation of the type in UnrealScript.</param>
        /// <returns></returns>
        private string GetType(string type)
        {
            switch (type)
            {
                // Variable is an array.
                case "TArray":
                    return "array";

                // Variable is a boolean.
                case "long":
                    return "bool";

                // Variable is a script delegate.
                case "FScriptDelegate":

                    // Throw variable creation exception, delegates break UDK compilation.
                    throw new InvalidVariableException();

                // Variable is a byte.
                case "char":
                    return "byte";

                // Variable is a string.
                case "FString":
                    return "string";

                // Variable is a name.
                case "FName":
                    return "name";

                // Variable is a known primitive or a class reference.
                default:

                    // If the first letter is a capital then it's a class reference and will have a prefix that needs to be removed.
                    if(Char.IsUpper(type[0]))
                    {
                        type = type.Remove(0, 1);
                    }
                    return type;
            }
        }

        /// <summary>
        /// Returns the name of a given variable.
        /// </summary>
        /// <param name="headerDefinition">The definition of this variable provided by the header.</param>
        /// <returns></returns>
        private string GetVariableName(string headerDefinition)
        {
            // Split when there have been three spaces in a row (three because when the string is parsed arrays have spaces, so removing their symbols creates 2 spaces).
            List<string> splitDefinition = Regex.Split(headerDefinition, @" {3,}").ToList<string>();
            splitDefinition.RemoveAll(item => item == "");

            // Get the name of the variable.
            string name = null;
            name = splitDefinition[1];

            // Return the formatted type.
            return name;
        }

        /// <summary>
        /// Sets the modifiers of the variable by splitting the definition in the right spots.
        /// </summary>
        /// <param name="headerDefinition">The definition of this variable provided by the header.</param>
        private void SetModifiers(string headerDefinition)
        {
            // Create the Modifiers list. Needs to be created even if none exist or it cannot be accessed for printing.
            this.Modifiers = new List<VariableModifier>();

            // Get the modifiers of the variable.
            string[] definitionParts = headerDefinition.Split(new Char[] { '(', ')' });
            if (definitionParts.Length < 4)
            {
                // No modifiers present, exit function.
                return;
            }

            // Get the modifiers individually.
            List<string> modifiers = definitionParts[3].Split(new Char[] { ' ', '|' }).ToList<string>();

            // Remove empty string entries.
            modifiers.RemoveAll(entry => entry == "");

            // Add each modifier.
            foreach (string modifier in modifiers)
            {
                switch(modifier)
                {
                    case "CPF_Edit":
                        this.Modifiers.Add(VariableModifier.Edit);
                        break;
                    case "CPF_Const":
                        this.Modifiers.Add(VariableModifier.Const);
                        break;
                    case "CPF_EditConst":
                        this.Modifiers.Add(VariableModifier.EditConst);
                        break;
                    case "CPF_Transient":
                        this.Modifiers.Add(VariableModifier.Transient);
                        break;
                    case "CPF_Export":
                        if (!this.Modifiers.Contains(VariableModifier.Export))
                        {
                            this.Modifiers.Add(VariableModifier.Export);
                        }
                        break;
                    case "CPF_ExportObject":
                        if (!this.Modifiers.Contains(VariableModifier.Export))
                        {
                            this.Modifiers.Add(VariableModifier.Export);
                        }
                        break;
                    case "CPF_EditInline":
                        this.Modifiers.Add(VariableModifier.EditInline);
                        break;
                    case "CPF_EditInlineNotify":
                        this.Modifiers.Add(VariableModifier.EditInlineNotify);
                        break;
                    case "CPF_Native":
                        this.Modifiers.Add(VariableModifier.Native);
                        break;
                    case "CPF_Localized":
                        this.Modifiers.Add(VariableModifier.Localized);
                        break;
                    case "CPF_Net":
                        this.Modifiers.Add(VariableModifier.Net);
                        break;
                    case "CPF_EditConstArray":
                        this.Modifiers.Add(VariableModifier.EditConstArray);
                        break;
                    case "CPF_NoExport":
                        this.Modifiers.Add(VariableModifier.NoExport);
                        break;
                    case "CPF_Config":
                        // Do nothing. No need.
                        break;
                    case "CPF_Component":
                        // Do nothing. No need.
                        break;
                    case "CPF_NeedCtorLink":
                        // Means the header has no reference to the type specified. Ignore.
                        break;
                    default:
                        Console.WriteLine("Modifier '{0}' not recognised.", modifier);
                        break;
                }
            }
        }

        /// <summary>
        /// Turns the variable into an UnrealScript variable definition ended by a new line character.
        /// </summary>
        /// <returns>A formatted variable definition.</returns>
        public override string ToString()
        {
            string variableDefinition = "";
            variableDefinition = this.Modifiers.Contains(VariableModifier.Edit) ? "var()" : "var";

            // Add modifiers.
            foreach (VariableModifier modifier in this.Modifiers)
            {
                switch (modifier)
                {
                    case VariableModifier.Const:
                        variableDefinition += " const";
                        break;
                    case VariableModifier.EditConst:
                        variableDefinition += " editconst";
                        break;
                    case VariableModifier.EditConstArray:
                        variableDefinition += " editconstarray";
                        break;
                    case VariableModifier.EditInline:
                        variableDefinition += " editinline";
                        break;
                    case VariableModifier.EditInlineNotify:
                        variableDefinition += " databinding";
                        break;
                    case VariableModifier.Export:
                        variableDefinition += " export";
                        break;
                    case VariableModifier.Transient:
                        variableDefinition += " transient";
                        break;
                    case VariableModifier.Native:
                        variableDefinition += " native";
                        break;
                    case VariableModifier.Net:
                        variableDefinition += " repnotify";
                        break;
                    case VariableModifier.NoExport:
                        variableDefinition += " noexport";
                        break;
                }
            }

            // Add type.
            variableDefinition += " " + this.Type;

            // Add name.
            variableDefinition += " " + this.Name;

            // Close line, add new line.
            variableDefinition += ";" + '\n';

            return variableDefinition;
        }
    }
}
