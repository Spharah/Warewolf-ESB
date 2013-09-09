﻿using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public abstract class AbstractResourceMessage : IResourceMessage
    {
        public IResourceModel ResourceModel { get; set; }

        protected AbstractResourceMessage(IResourceModel resourceModel)
        {
            ResourceModel = resourceModel;
        }
    }
}
