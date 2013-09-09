﻿using System;
using System.Linq;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Unlimited.Framework.Converters.Graph.Poco
{
    public class PocoInterrogator : IInterrogator
    {
        #region Methods

        public IMapper CreateMapper(object data)
        {
            return new PocoMapper();
        }

        public INavigator CreateNavigator(object data, Type pathType)
        {
            if (!pathType.GetInterfaces().Contains(typeof(IPath)))
            {
                throw new Exception("'" + pathType.ToString() + "' doesn't implement '" + typeof(IPath).ToString() + "'");
            }

            return new PocoNavigator(data);
        }

        #endregion Methods
    }
}
