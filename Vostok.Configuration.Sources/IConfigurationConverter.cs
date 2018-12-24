using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources
{
    // CR(krait): What's the purpose of this interface?
    public interface IConfigurationConverter<in T>
    {
        ISettingsNode Convert(T configuration);
    }
}