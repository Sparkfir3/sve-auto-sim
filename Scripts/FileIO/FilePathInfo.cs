using System.IO;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    [CreateAssetMenu(menuName = "SVE Simulator/File Path Info", fileName = "FilePathInfo", order = 1)]
    public class FilePathInfo : ScriptableObject
    {
        [BoxGroup("Path Segments"), SerializeField]
        private string dataFolder = "SVESimData";
        [BoxGroup("Path Segments"), SerializeField]
        private string cardDataFolder = "CardData";
        [BoxGroup("Path Segments"), SerializeField]
        private string imagesFolder = "Images";
        [BoxGroup("Path Segments"), SerializeField]
        private string miscFolder = "Other";
        [BoxGroup("Path Segments"), SerializeField]
        private string decksFolder = "Decks";

        [BoxGroup("File Paths"), SerializeField, ReadOnly]
        private string cardDataPath;
        [BoxGroup("File Paths"), SerializeField, ReadOnly]
        private string imagesPath;
        [BoxGroup("File Paths"), SerializeField, ReadOnly]
        private string miscImagesPath;
        [BoxGroup("File Paths"), SerializeField, ReadOnly]
        private string decksPath;

        [field: BoxGroup("File Info"), SerializeField]
        public string DefaultFileExtension { get; private set; } = ".png";
        [BoxGroup("File Info"), SerializeField]
        private string cardBackFileName = "CardBack";
        [field: BoxGroup("File Info"), SerializeField, ReadOnly]
        public string CardBackFile { get; private set; }
        [BoxGroup("File Info"), SerializeField]
        private string evolvePointFileName = "EvolvePoint";
        [field: BoxGroup("File Info"), SerializeField, ReadOnly]
        public string EvolvePointFile { get; private set; }
        [field: BoxGroup("File Info"), SerializeField]
        public string DatabaseVersionFileName { get; private set; } = "db_version.txt";

        public string MainFolderPath => Path.Combine(Application.persistentDataPath, dataFolder);
        public string CardDataPath => Path.Combine(Application.persistentDataPath, cardDataPath);
        public string ImagesPath => Path.Combine(Application.persistentDataPath, imagesPath);
        public string MiscImagesPath => Path.Combine(Application.persistentDataPath, miscImagesPath);
        public string DeckFolderPath => Path.Combine(Application.persistentDataPath, decksPath);

        public string BuildFolderImagesPath => Path.Combine(Application.dataPath, imagesPath);

        public string DatabaseVersionFilePath => Path.Combine(MainFolderPath, DatabaseVersionFileName);

        // ------------------------------

#if UNITY_EDITOR
        private void OnValidate()
        {
            CardBackFile = cardBackFileName + DefaultFileExtension;
            EvolvePointFile = evolvePointFileName + DefaultFileExtension;

            cardDataPath = Path.Combine(dataFolder, cardDataFolder);
            imagesPath = Path.Combine(dataFolder, imagesFolder);
            miscImagesPath = Path.Combine(imagesPath, miscFolder);
            decksPath = Path.Combine(dataFolder, decksFolder);
        }
#endif
    }
}
