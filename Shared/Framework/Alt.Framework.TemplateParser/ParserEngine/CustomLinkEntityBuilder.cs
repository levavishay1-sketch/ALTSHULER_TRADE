using Alt.Framework.TemplateParser.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.Framework.TemplateParser.ParserEngine
{
    internal class CustomLinkEntityBuilder
    {
        private Dictionary<string, SpecialOperationPlaceHolderCollection> PlaceHolders;
        private CustomEntity regardingObject;

        internal CustomLinkEntityBuilder(Dictionary<string, SpecialOperationPlaceHolderCollection> placeHolders, CustomEntity regardingObject)
        {
            this.PlaceHolders = placeHolders;
            this.regardingObject = regardingObject;
        }

        internal void HandleCreateCustomLinkEntitiesPlaceHolders(CustomLinkEntity customLinkEntity, CustomEntity customEntity, List<string> attributesToSelect)
        {
            if (attributesToSelect != null && customLinkEntity != null)
            {

                this.regardingObject = customEntity;
                foreach (var attribute in attributesToSelect)
                {
                    if (attribute.Split('>').Length > 1)
                    {
                        this.CreateCustomLinkEntityBySpecificPlaceHolder(attribute, customLinkEntity);
                    }
                    else
                    {
                        customLinkEntity.Attributes.Add(attribute);
                    }
                }
            }
        }

        internal CustomLinkEntity HandleCreateCustomLinkEntitiesByPlaceHolders()
        {
            foreach (var placeHolder in this.PlaceHolders)
            {
                if ((placeHolder.Value != null && placeHolder.Value.Contains(t => t.Value.IsValidToParse)) || placeHolder.Value == null)
                {
                    if (placeHolder.Key.Split('>').Length > 1)
                    {
                        this.CreateCustomLinkEntityBySpecificPlaceHolder(placeHolder.Key, regardingObject);
                    }
                    else // entity field
                    {
                        regardingObject.Attributes.Add(placeHolder.Key);
                    }
                }
            }
            return regardingObject as CustomLinkEntity;
        }

        public void CreateCustomLinkEntityBySpecificPlaceHolder(string aliasPlaceHolder, CustomEntity root)
        {
            string[] placeHolderEntityStages = aliasPlaceHolder.Split('>');
            aliasPlaceHolder = aliasPlaceHolder.Replace(">", ".");
            var attribute = aliasPlaceHolder.LastIndexOf(".");
            aliasPlaceHolder = aliasPlaceHolder.Remove(attribute);
            int placeHoldersLength = placeHolderEntityStages.Length - 1;

            for (int i = 0; i < placeHoldersLength; i++)
            {
                if (i == 0)
                {
                    root = this.CreateCustomLinkEntityByEntityStage(aliasPlaceHolder, root as CustomLinkEntity, i, placeHolderEntityStages, placeHoldersLength);
                }
                else
                    if (i == placeHoldersLength)
                {
                    break;
                }
                else // i = 1 and greater
                {
                    root = this.CreateCustomLinkEntityByEntityStage(aliasPlaceHolder, root as CustomLinkEntity, i, placeHolderEntityStages, placeHoldersLength);
                }
            }
        }

        private CustomLinkEntity CreateCustomLinkEntityByEntityStage(string aliasPlaceHolder, CustomLinkEntity root, int i, string[] placeHolderEntityStages, int placeHoldersLength)
        {
            if (root == null)
            {
                throw new Exception("invalid place holder");
            }

            string fromAttribute = string.Empty;
            string[] nextEntity = placeHolderEntityStages[i + 1].Split('.');
            string[] currentEntity = placeHolderEntityStages[i].Split('.');

            //get the from attribute base on index of placeHolder
            fromAttribute = i == 0 ? placeHolderEntityStages[i] : currentEntity[1];

            var existedNode = root.LinkEntities.FirstOrDefault(n => n.LinkFromAttributeName == fromAttribute);
            if (existedNode == null)
            {
                if (i + 1 == placeHoldersLength)
                {
                    existedNode = new CustomLinkEntity();
                    existedNode.LinkFromEntityName = i == 0 ? this.regardingObject.EntityName : currentEntity[0];
                    existedNode.LinkFromAttributeName = fromAttribute;
                    existedNode.LinkToEntityName = nextEntity[0];
                    existedNode.LinkToAttributeName = $"{nextEntity[0]}id";
                    existedNode.Attributes.Add(nextEntity[1]);
                    root.LinkEntities.Add(existedNode);
                    //add alias
                    existedNode.Alias = aliasPlaceHolder;
                }
                else
                {
                    //fill  linkFromEntityName and linkToAttributeName based on indes of place holder
                    string nextEntityName = nextEntity[0];
                    string linkFromEntityName = i == 0 ? this.regardingObject.EntityName : currentEntity[0];
                    string linkToAttributeName = i == 0 ? $"{nextEntityName}id" : $"{nextEntityName}id";//nextEntity[1];
                    //create new node
                    existedNode = new CustomLinkEntity(linkFromEntityName, fromAttribute, nextEntityName, linkToAttributeName);
                    root.LinkEntities.Add(existedNode);
                }
            }
            else
            {
                if (i + 1 == placeHoldersLength)
                {
                    existedNode.Attributes.Add(nextEntity[1]);
                    //add alisa Alias
                    existedNode.Alias = aliasPlaceHolder;
                }
            }

            return existedNode;
        }
    }
}
