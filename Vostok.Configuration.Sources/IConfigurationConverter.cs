using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources
{
    public interface IConfigurationConverter<in T>
    {
        ISettingsNode Convert(T configuration);
    }
}