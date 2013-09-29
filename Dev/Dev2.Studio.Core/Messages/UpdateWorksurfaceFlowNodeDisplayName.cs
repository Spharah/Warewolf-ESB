﻿using System;

namespace Dev2.Messages
{
    public class UpdateWorksurfaceFlowNodeDisplayName
    {
        public UpdateWorksurfaceFlowNodeDisplayName(Guid worksurfaceResourceID, string oldName, string newName)
        {
            WorksurfaceResourceID = worksurfaceResourceID;
            OldName = oldName;
            NewName = newName;
        }

        public string OldName { get; set; }
        public string NewName { get; set; }
        public Guid WorksurfaceResourceID { get; set; }
    }
}
