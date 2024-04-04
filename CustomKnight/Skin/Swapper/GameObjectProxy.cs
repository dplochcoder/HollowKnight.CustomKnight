using System.IO;

namespace CustomKnight
{
    public class GameObjectProxy
    {
        public string name;
        public string alias = "spl_ck_dndy";
        public string rootPath; //rootPath/name.png
        public bool hasTexture;
        public bool hasChildren;
        public string fileType;
        public Dictionary<string, GameObjectProxy> children = new Dictionary<string, GameObjectProxy>();

        public string getObjectPath()
        {
            return Path.Combine(rootPath, name + fileType);
        }
        public string getTexturePath()
        {
            return Path.Combine(rootPath, name + ".png");
        }
        public string getAliasPath()
        {
            if (alias != "spl_ck_dndy")
            {
                CustomKnight.Instance.Log(alias);
            }
            return Path.Combine(rootPath, alias + ".png");
        }
        public void TraverseGameObjectPath(string path, string rootPath, string name, string alias = "spl_ck_dndy")
        {
            CustomKnight.Instance.LogDebug($"{path}:{rootPath}:{name}");
            var pathSplit = path.Split(new Char[] { '/' }, 3);
            GameObjectProxy GOP = null;
            hasChildren = false;
            if (pathSplit.Length > 1)
            {
                hasChildren = true;
                if (!children.TryGetValue(pathSplit[1], out GOP))
                {
                    GOP = new GameObjectProxy()
                    {
                        name = pathSplit[1],
                        hasTexture = false,
                    };
                }
                children[pathSplit[1]] = GOP;
            }
            if (GOP != null)
            {
                if (pathSplit.Length > 2)
                {
                    GOP.TraverseGameObjectPath($"{pathSplit[1]}/{pathSplit[2]}", rootPath, name, alias);
                }
                else
                {
                    if (!GOP.hasTexture)
                    { // do not over ride existing texture
                        GOP.hasTexture = true;
                        GOP.rootPath = rootPath;
                        GOP.name = name;
                        GOP.alias = alias;
                    }
                }
            }
            else
            {
                if (!this.hasTexture)
                {
                    this.hasTexture = true;
                    this.rootPath = rootPath;
                    this.name = name;
                    this.alias = alias;
                }
            }

            CustomKnight.Instance.LogDebug($"{this.hasTexture}:{this.rootPath}:{this.name}:{this.alias}:{(this.rootPath == null ? "null root" : "x")}");
        }
        public void TraverseGameObjectDirectory(string basePath)
        {
            var path = Path.Combine(basePath, Path.Combine(rootPath, name));
            if (!Directory.Exists(path))
            {
                hasChildren = false;
                return;
            }
            // check if it has files
            foreach (string file in Directory.GetFiles(path))
            {
                string filename = Path.GetFileName(file);
                //Log(filename);
                if (!filename.EndsWith(".txt"))
                {
                    string extension = Path.GetExtension(file);
                    string objectName = filename.Replace(extension, "");
                    GameObjectProxy GOP = new GameObjectProxy()
                    {
                        name = objectName,
                        hasTexture = true,
                        rootPath = Path.Combine(rootPath, name),
                        hasChildren = false,
                        fileType = extension
                    };
                    hasChildren = true;
                    children[objectName] = GOP;
                }
            }
            // check if it has directories
            foreach (string directory in Directory.GetDirectories(path))
            {
                string directoryName = new DirectoryInfo(directory).Name;
                //Log(directoryName);
                GameObjectProxy GOP;
                if (!children.TryGetValue(directoryName, out GOP))
                {
                    GOP = new GameObjectProxy()
                    {
                        name = directoryName,
                        hasTexture = false,
                        rootPath = Path.Combine(rootPath, name),
                        hasChildren = true
                    };
                }
                hasChildren = true;
                children[directoryName] = GOP;
                if (GOP.rootPath == "" || GOP.rootPath == null)
                {
                    GOP.rootPath = Path.Combine(rootPath, name);
                }
                GOP.TraverseGameObjectDirectory(basePath);
            }

        }
    }
}