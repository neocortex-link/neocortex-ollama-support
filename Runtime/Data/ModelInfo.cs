namespace Neocortex.Data
{
    public class ModelInfo
    {
        public string model;
        public string parameters;
        public string size;
        public string name;
        public bool isDownloaded;

        public string DisplayName => $"{model} {parameters} ({size}) {(isDownloaded ? " [Downloaded]" : "")}";
    }
}