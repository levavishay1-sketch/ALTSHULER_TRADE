using Alt.DataModel.Crm.Core.Interfaces;
using Alt.Framework.TemplateParser.Interfaces;
using Alt.Framework.TemplateParser.Models;
using Alt.Framework.TemplateParser.SpecialOperations;
using Alt.Framework.TemplateParser.ValueResolvers;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace Alt.Framework.TemplateParser.ParserEngine
{
    public class Engine
    {
        private CustomEntity regardingObject;
        private ParserSettings parserSettings;
        private string crmUrl;
        private IEntityValueResolver entityValueResolver;
        public string Message { get; set; }

        // for normal place holders
        private Dictionary<string, SpecialOperationPlaceHolderCollection> PlaceHolders { get; set; } = new Dictionary<string, SpecialOperationPlaceHolderCollection>();

        private Dictionary<string, SpecialOperationPlaceHolder> TablePlaceHolders { get; set; } = new Dictionary<string, SpecialOperationPlaceHolder>();

        private Dictionary<string, HashSet<string>> originalRegularPlaceHoldersWithHtml { get; set; } = new Dictionary<string, HashSet<string>>();


        public List<SpecialOperationPlaceHolder> SupportedSpecialOperationPlaceHolders = new List<SpecialOperationPlaceHolder>();

        public Engine(string EntityName, string Message, string id, string crmUrl, IEntityValueResolver entityValueResolver = null)
        {
            regardingObject = new CustomLinkEntity();
            regardingObject.EntityName = EntityName;
            this.Message = Message;
            regardingObject.Id = id;
            this.crmUrl = crmUrl;
            this.entityValueResolver = entityValueResolver ?? new DefaultEntityValueResolver();
        }

        public Engine(ParserSettings parserSettings)
        {
            if (parserSettings == null)
            {
                throw new Exception("ParserSettings object can't be null");
            }

            if (string.IsNullOrWhiteSpace(parserSettings.RegardingObjectId))
            {
                throw new Exception("RegardingObjectId cant be null");
            }
            this.parserSettings = parserSettings;
            regardingObject = new CustomLinkEntity
            {
                EntityName = parserSettings.RegardingObjectEntityLogicalName,
                Id = parserSettings.RegardingObjectId.ToString()
            };

            this.Message = parserSettings.DecodeMssageForHtmlParsing ? HttpUtility.HtmlDecode(parserSettings.MessageToParse) : parserSettings.MessageToParse;
            this.crmUrl = parserSettings.CrmUrl;
            this.entityValueResolver = parserSettings.EntityValueResolver ?? new DefaultEntityValueResolver();
        }

        public virtual CustomLinkEntity InitiateCustomLinkEntityBuilder()
        {
            this.ExtractPlaceHoldersFromMessage();
            CustomLinkEntityBuilder customLinkEntityBuilder = new CustomLinkEntityBuilder(this.PlaceHolders, this.regardingObject);
            return customLinkEntityBuilder.HandleCreateCustomLinkEntitiesByPlaceHolders();
        }

        public virtual string ParseEntitiesToMessage(IEnumerable<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                this.AddKeysThatGotNullValue(entity);
                foreach (var key in entity.Attributes.Keys)
                {
                    string placrHolder = this.ConvertToOriginalPlaceHolder(key);
                    var KeyValuePair = new KeyValuePair<string, Entity>(key, entity);
                    this.HandleReplaceParsedAttributeInMessage(placrHolder, KeyValuePair);
                }
            }
            this.HandleParseEmtpyAndInvalidPlaceHoldersInMessage();
            return this.Message;
        }

        private void AddKeysThatGotNullValue(Entity entity)
        {
            foreach (var placrHolder in this.PlaceHolders)
            {
                var entityKey = placrHolder.Key.Replace(">", ".");
                if (!entity.Contains(entityKey))
                {
                    entity.Attributes.Add(entityKey, null);
                }
            }
        }

        protected virtual string ConvertToOriginalPlaceHolder(string attributeKey)
        {
            string[] placeholder = attributeKey.Split('.');
            var builder = new StringBuilder();
            if (placeholder.Length == 1)
            {
                builder.Append(placeholder[0]);
            }
            else
            {
                builder.Append(placeholder[0]);
                for (int i = 1; i < placeholder.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        builder.Append(".").Append(placeholder[i]);
                    }
                    else
                    {
                        builder.Append(">").Append(placeholder[i]);

                    }
                }
            }
            return builder.ToString();
        }

        protected virtual void HandleReplaceParsedAttributeInMessage(string placeHolder, KeyValuePair<string, Entity> keyValuePair)
        {
            StringBuilder validPlaceHolderInMessage = new StringBuilder();

            if (this.PlaceHolders.ContainsKey(placeHolder))
            {
                string placeHolderValue = this.entityValueResolver.GetAttributeValue(keyValuePair.Key, keyValuePair.Value);
                if (this.PlaceHolders[placeHolder] != null)
                {
                    SpecialOperationPlaceHolderCollection specialOperationPlaceHolderCollection = this.PlaceHolders[placeHolder];
                    foreach (var specialOperationPlaceHolder in specialOperationPlaceHolderCollection)
                    {
                        string specialPlaceHolderValue = this.GetPlaceHolderSpecialOperationValue(keyValuePair.Key, keyValuePair.Value, specialOperationPlaceHolder.Value) ?? placeHolderValue;
                        if (specialOperationPlaceHolder.Value.SpecialOperation.SpecialOperationType == SpecialOperationType.RegualrWithInlineOperation)
                        {
                            //if (!specialPlaceHolderValue.Contains("?") && !specialPlaceHolderValue.Contains(":"))
                            InlineConditionPlaceHolder inlineConditionPlaceHolderAfterParse = JsonSerializer.Deserialize<InlineConditionPlaceHolder>(specialOperationPlaceHolder.Value.Content);

                            if (inlineConditionPlaceHolderAfterParse.IsAllSubPlaceHoldersParsed)// check if Ifcondition result finish parsing all its  placeholders and get the condition result
                            {
                                InlineConditionPlaceHolder inlineConditionPlaceHolder = JsonSerializer.Deserialize<InlineConditionPlaceHolder>(specialOperationPlaceHolder.Key);
                                validPlaceHolderInMessage.Append("@{").Append(inlineConditionPlaceHolder.OriginalInlineConditionPlaceHolder).Append("}@");
                                this.Message = specialOperationPlaceHolder.Value.SpecialOperation.ReplaceResultInOriginalMessage(this.Message, validPlaceHolderInMessage?.ToString(), specialOperationPlaceHolder.Value, specialPlaceHolderValue);
                                validPlaceHolderInMessage.Clear();
                            }
                        }
                        else
                        {
                            validPlaceHolderInMessage.Append("@{").Append($"{specialOperationPlaceHolder.Value.Prefix}{specialOperationPlaceHolder.Value.Content}{specialOperationPlaceHolder.Value.Suffix}").Append("}@");
                            this.Message = specialOperationPlaceHolder.Value.SpecialOperation.ReplaceResultInOriginalMessage(this.Message, validPlaceHolderInMessage?.ToString(), specialOperationPlaceHolder.Value, specialPlaceHolderValue);
                            validPlaceHolderInMessage.Clear();
                        }
                    }
                }

                if (parserSettings.RemoveHtmlTagsInRegularPlaceHolders && originalRegularPlaceHoldersWithHtml.ContainsKey(placeHolder))
                { // get the original regular placeholder with html 
                    HashSet<string> originalRegularPlaceHoldersWithHtmlHashSet = originalRegularPlaceHoldersWithHtml[placeHolder];
                    foreach (var riginalRegularPlaceHolder in originalRegularPlaceHoldersWithHtmlHashSet)
                    {
                        validPlaceHolderInMessage.Clear().Append("@{").Append(riginalRegularPlaceHolder).Append("}@");
                        this.Message = this.Message.Replace(validPlaceHolderInMessage?.ToString(), placeHolderValue);// replace the rest of keys if there is other html placeholders
                    }
                }
                else
                {
                    validPlaceHolderInMessage.Clear().Append("@{").Append(placeHolder).Append("}@");
                    this.Message = this.Message.Replace(validPlaceHolderInMessage?.ToString(), placeHolderValue);// replace the rest of keys
                }

                //remove the parssed key
                this.PlaceHolders.Remove(placeHolder);
            }
        }

        protected virtual void HandleParseEmtpyAndInvalidPlaceHoldersInMessage()
        {
            List<string> placeholders = new List<string>();
            var checkText = this.AddFirstInstanceOfPlaceHodlderToPlaceHoldersList(this.Message, "@{", "}@", placeholders);
            while (checkText != null)
            {
                checkText = this.AddFirstInstanceOfPlaceHodlderToPlaceHoldersList(checkText, "@{", "}@", placeholders);
            }

            foreach (var placeholder in placeholders)
            {
                this.Message = this.Message.Replace($"@{{{placeholder}}}@",this.parserSettings.ValueToParseInEmptyOrInvalidPlaceHolders);
            }
        }

        protected virtual void HandleParseEmtpyAndInvalidPlaceHoldersInMessageOld()
        {
            foreach (var emptyValuePlaceHolder in this.PlaceHolders)
            {
                if (this.Message.Contains(emptyValuePlaceHolder.Key))
                {
                    StringBuilder placeHolderToReplace = new StringBuilder();
                    string placeHolderValue = string.Empty;
                    if (emptyValuePlaceHolder.Value == null)
                    {
                        placeHolderToReplace.Append("@{").Append(emptyValuePlaceHolder.Key).Append("}@");
                        this.Message = this.Message.Replace(placeHolderToReplace.ToString(), placeHolderValue);
                    }
                    else
                    {
                        foreach (var emptySpecialOperationPlaceHolder in emptyValuePlaceHolder.Value)
                        {
                            if (!emptySpecialOperationPlaceHolder.Value.IsValidToParse)
                            {
                                placeHolderValue = this.GetPlaceHolderSpecialOperationValue(null, null, emptySpecialOperationPlaceHolder.Value);
                                placeHolderToReplace.Append("@{").Append(emptyValuePlaceHolder.Key).Append("}@");
                            }
                            else
                            {

                                placeHolderToReplace.Append("@{").Append(emptySpecialOperationPlaceHolder.Value.Prefix).Append(emptyValuePlaceHolder.Key).Append(emptySpecialOperationPlaceHolder.Value.Suffix).Append("}@");
                            }

                            this.Message = this.Message.Replace(placeHolderToReplace.ToString(), placeHolderValue);
                        }
                    }
                }
            }
        }


        protected virtual void ExtractPlaceHoldersFromMessage()
        {
            var text = this.Message;
            //List<string> placeholders = GetPlaceHoldersValues(this.Message, "@{", "}@").Where(t => !string.IsNullOrWhiteSpace(t.Trim())).ToList();
            List<string> placeholders = new List<string>();
            var checkText = this.AddFirstInstanceOfPlaceHodlderToPlaceHoldersList(text, "@{", "}@", placeholders);
            while (checkText != null)
            {
                checkText = this.AddFirstInstanceOfPlaceHodlderToPlaceHoldersList(checkText, "@{", "}@", placeholders);
            }

            foreach (var placeholder in placeholders)
            {
                string placeolderWithNoHtml = parserSettings != null && parserSettings.RemoveHtmlTagsInRegularPlaceHolders ? StripHTML(placeholder) : placeholder;

                if (!this.PlaceHolders.ContainsKey(placeolderWithNoHtml) && !string.IsNullOrWhiteSpace(placeolderWithNoHtml))
                {
                    var specialOperationFromPlaceHolder = this.ExtractSpecialOperationFromPlaceHolder(placeholder);
                    if (specialOperationFromPlaceHolder != null)
                    {
                        if (specialOperationFromPlaceHolder.IsTablePlaceHolder)
                        {
                            if (!this.TablePlaceHolders.ContainsKey(specialOperationFromPlaceHolder.Content))
                            {
                                this.TablePlaceHolders.Add(specialOperationFromPlaceHolder.Content, specialOperationFromPlaceHolder);
                                string lookupToStartFrom = specialOperationFromPlaceHolder.Content.Split(',').Select(s => s.Trim()).FirstOrDefault();
                                var specialOperationPlaceHolderToAdd = new SpecialOperationPlaceHolder(specialOperationFromPlaceHolder, specialOperationFromPlaceHolder.Content);
                                this.AddToPlaceHoldersFromTableHandler(specialOperationPlaceHolderToAdd, lookupToStartFrom);
                            }
                        }
                        else
                        {
                            this.AddToPlaceHoldersFromTableHandler(specialOperationFromPlaceHolder);
                        }
                    }
                    else
                    {
                        if (parserSettings.RemoveHtmlTagsInRegularPlaceHolders && placeolderWithNoHtml != placeholder)
                        {
                            originalRegularPlaceHoldersWithHtml.Add(placeolderWithNoHtml, new HashSet<string> { placeholder });
                        }
                        this.PlaceHolders.Add(placeolderWithNoHtml, null);
                    }
                }
                else if (originalRegularPlaceHoldersWithHtml.ContainsKey(placeolderWithNoHtml) && !originalRegularPlaceHoldersWithHtml[placeolderWithNoHtml].Contains(placeholder))
                {
                    originalRegularPlaceHoldersWithHtml[placeolderWithNoHtml].Add(placeholder);
                }
            }
        }

        protected virtual string GetPlaceHolderSpecialOperationValue(string key, Entity entity, SpecialOperationPlaceHolder specialOperationPlaceHolder)
        {
            return specialOperationPlaceHolder?.SpecialOperation.ExecuteSpecialOperationLogic(entity, key, specialOperationPlaceHolder);
        }

        protected virtual string AddFirstInstanceOfPlaceHodlderToPlaceHoldersList(string text, string firstString, string lastString, List<string> placeholders)
        {
            string findedString = string.Empty;
            string result = string.Empty;

            int position1 = text.IndexOf(firstString) + firstString.Length;
            int position2 = text.IndexOf(lastString);

            if (position1 - 2 < 0 || position2 < 0)
            {
                return null;
            }

            if (position1 > position2)
            {
                return text.Remove(0, position1 - 2);
            }

            findedString = text.Substring(position1, position2 - position1);
            int indexOfSuffix = findedString.IndexOf("@{");
            if (indexOfSuffix > -1)
            {
                findedString = findedString.Remove(0, indexOfSuffix + 2);
            }

            placeholders.Add(findedString);

            result = text.Remove(position1 - 2, position2 - position1 + 4);
            return result;
        }

        protected virtual void AddToPlaceHoldersFromTableHandler(SpecialOperationPlaceHolder specialOperationPlaceHolder, string content = null)
        {
            string placeHolderContent = content ?? specialOperationPlaceHolder.Content;
            placeHolderContent = parserSettings != null && parserSettings.RemoveHtmlTagsInRegularPlaceHolders ? StripHTML(placeHolderContent) : placeHolderContent;

            if (specialOperationPlaceHolder.SpecialOperation.SpecialOperationType == SpecialOperationType.RegualrWithInlineOperation)
            {
                InlineConditionPlaceHolder inlineConditionPlaceHolder = JsonSerializer.Deserialize<InlineConditionPlaceHolder>(placeHolderContent);
                foreach (var inlineCondition in inlineConditionPlaceHolder.InlineConditions)
                {
                    string OperatorLeftSideKeyText = inlineCondition.ConditionLeftSideKey.Replace("{{", "").Replace("}}", "");
                    string OperatorRightSideKeyText = inlineCondition.ConditionRightSideKey.Replace("{{", "").Replace("}}", "");
                    this.AddToPlaceHoldersFromTable(specialOperationPlaceHolder, OperatorLeftSideKeyText);
                    if (inlineCondition.ConditionRightSideKey.Contains("{{"))
                    {
                        this.AddToPlaceHoldersFromTable(specialOperationPlaceHolder, OperatorRightSideKeyText);
                    }
                }

                string ConditionResultKeyText = inlineConditionPlaceHolder.ConditionsThenOperationtKey.Replace("{{", "").Replace("}}", "");
                string ConditionElseResultKeyText = inlineConditionPlaceHolder.ConditionsElseOperationtKey.Replace("{{", "").Replace("}}", "");

                if (inlineConditionPlaceHolder.ConditionsThenOperationtKey.Contains("{{"))
                {
                    this.AddToPlaceHoldersFromTable(specialOperationPlaceHolder, ConditionResultKeyText);
                }
                if (inlineConditionPlaceHolder.ConditionsElseOperationtKey.Contains("{{"))
                {
                    this.AddToPlaceHoldersFromTable(specialOperationPlaceHolder, ConditionElseResultKeyText);
                }
            }
            else
            {
                this.AddToPlaceHoldersFromTable(specialOperationPlaceHolder, placeHolderContent);
            }


        }

        private void AddToPlaceHoldersFromTable(SpecialOperationPlaceHolder specialOperationPlaceHolder, string placeHolderContent)
        {
            SpecialOperationPlaceHolderCollection specialOperationPlaceHolderCollection = null;
            if (!this.PlaceHolders.ContainsKey(placeHolderContent))
            {
                specialOperationPlaceHolderCollection = new SpecialOperationPlaceHolderCollection();
                //var specialOperationPlaceHolderToAdd = new SpecialOperationPlaceHolder(specialOperationPlaceHolder, specialOperationPlaceHolder.Content);
                specialOperationPlaceHolderCollection.Add(specialOperationPlaceHolder);
                this.PlaceHolders.Add(placeHolderContent, specialOperationPlaceHolderCollection);
            }
            else
            {
                if (this.PlaceHolders[placeHolderContent] == null)
                {// this occure when there is a regular place holder with the same attributekey as the special operation key so to prevent key dupplication error add the regular placeHolder as RegularParseOperation
                    specialOperationPlaceHolderCollection = new SpecialOperationPlaceHolderCollection();

                    //do assignment because in HandleReplaceParsedAttributeInMessage the parse continue to the regular place holder after parse the special operation
                    this.PlaceHolders[placeHolderContent] = specialOperationPlaceHolderCollection;
                }
                this.PlaceHolders[placeHolderContent].Add(specialOperationPlaceHolder);
            }

        }


        protected virtual SpecialOperationPlaceHolder ExtractSpecialOperationFromPlaceHolder(string placeHolder)
        {
            SpecialOperationPlaceHolder specialOperationPlaceHolder = null;
            foreach (var placeholderStructure in SupportedSpecialOperationPlaceHolders)
            {
                if (placeHolder == $"{placeholderStructure.Prefix}{placeholderStructure.Suffix}")
                {
                    specialOperationPlaceHolder = new SpecialOperationPlaceHolder(placeholderStructure, placeHolder);
                    specialOperationPlaceHolder.IsValidToParse = false;
                }

                string result = placeholderStructure.SpecialOperation.GetTextContentFromSpecialOperationPattern(placeHolder, placeholderStructure.Prefix, placeholderStructure.Suffix);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    specialOperationPlaceHolder = new SpecialOperationPlaceHolder(placeholderStructure, result);
                }
            }
            return specialOperationPlaceHolder;
        }

        protected virtual string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

    }
}
