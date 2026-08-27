using Alt.DataModel.Crm.Core.Contracts;
using Alt.Framework.Mapper;
using Microsoft.Xrm.Sdk;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiContext<TApiEntity> where TApiEntity : ApiEntityBase
    {
        public TApiEntity Target { get; set; }
        public TApiEntity PreImage { get; set; }
        public TApiEntity PostImage { get; set; }
        public TApiEntity MergedTarget { get; set; }
        public string MessageName { get; set; }

        public bool IsContextContainsTarget { get; set; }


        public ApiContext(Entity target, Entity preImage, Entity postImage, string messageName, bool isContextContainsTarget = false)
        {
            CrmEntityMapper<TApiEntity> crmEntityMapper = new CrmEntityMapper<TApiEntity>();

            this.Target = target != null ? crmEntityMapper.MappCrmEntityToApiEntity(target) : null;
            this.PreImage = preImage != null ? crmEntityMapper.MappCrmEntityToApiEntity(preImage) : null;
            this.PostImage = postImage != null ? crmEntityMapper.MappCrmEntityToApiEntity(postImage) : null;
            this.MergedTarget = isContextContainsTarget ? crmEntityMapper.MappCrmEntityToApiEntity(Merge(target, preImage)) : null;
            this.MessageName = messageName;
            this.IsContextContainsTarget = isContextContainsTarget;
        }

        public ApiContext(TApiEntity target, string messageName)
        {
            this.Target = this.MergedTarget = this.PreImage = target;
            this.MessageName = messageName;
            this.IsContextContainsTarget = true;
        }

        public T Merge<T>(T target, T image) where T : Entity, new()
        {
            Entity mergedEntity = new Entity(target.LogicalName, target.Id);
            if (image != null)
            {
                Entity privateImage = image as Entity;
                foreach (var attrib in privateImage.Attributes)
                {
                    mergedEntity.Attributes[attrib.Key] = attrib.Value;
                }

                foreach (var formatedValue in privateImage.FormattedValues)
                {
                    mergedEntity.FormattedValues[formatedValue.Key] = formatedValue.Value;
                }
            }

            if (target != null)
            {
                Entity privateTarget = target as Entity;

                foreach (var attrib in privateTarget.Attributes)
                {
                    mergedEntity.Attributes[attrib.Key] = attrib.Value;
                }

                foreach (var formatedValue in privateTarget.FormattedValues)
                {
                    mergedEntity.FormattedValues[formatedValue.Key] = formatedValue.Value;
                }
            }
            return mergedEntity.ToEntity<T>();
        }
    }
}
