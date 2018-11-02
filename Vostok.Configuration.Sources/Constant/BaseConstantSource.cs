using System;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources.Constant
{
    public class BaseConstantSource : ConfigurationSourceAdapter
    {
        protected BaseConstantSource(Func<ISettingsNode> getSettings)
            : base(new BaseConstantRawSource(getSettings))
        {
        }
    }
}