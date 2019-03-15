
using System;

namespace BlueberryTexturePackerCore
{
    public class MainService
    {
        private ProjectModel project;

        //public PackerPreviewModel PackerPreviewModel { get; }
        //public PackerWindowModel PackerWindowModel { get; }
        public ProjectModel ProjectModel
        {
            get => project;
            set
            {
                project = value;
                OnProjectChanged?.Invoke(value);
            }
        }

        public event Action<ProjectModel> OnProjectChanged;

        public MainService()
        {
            //PackerPreviewModel = new PackerPreviewModel();
            //PackerWindowModel = new PackerWindowModel();

            /*PackerWindowModel.OnSelectedAtlasChanged += (atlas) =>
            {
                PackerPreviewModel.Atlas = atlas;
            };*/
        }
    }
}
