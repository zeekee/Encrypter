using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Encrypter
{
    public partial class Form1 : Form
    {
        string[] filesPathToEncryp;
        string[] filesPathToDecryp;
        string key1PathToDecryp;
        string key2PathToDecryp;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listBox1.AllowDrop = true;
            listBox1.DragEnter += new DragEventHandler(DragEnter);
            listBox1.DragDrop += new DragEventHandler(DragDrop);

            listBox2.AllowDrop = true;
            listBox2.DragEnter += new DragEventHandler(DragEnter);
            listBox2.DragDrop += new DragEventHandler(DragDrop);

            key1Box.AllowDrop = true;
            key1Box.DragEnter += new DragEventHandler(DragEnter);
            key1Box.DragDrop += new DragEventHandler(DragDrop);

            key2Box.AllowDrop = true;
            key2Box.DragEnter += new DragEventHandler(DragEnter);
            key2Box.DragDrop += new DragEventHandler(DragDrop);
        }

        public void DragEnter(Object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        public void DragDrop(Object sender, DragEventArgs e)
        {
            ListBox listBox = (ListBox)sender;
            string[] filesPath = (string[])e.Data.GetData(DataFormats.FileDrop);

            switch (listBox.Name)
            {
                case "listBox1":
                    filesPathToEncryp = filesPath;
                    break;
                case "listBox2":
                    filesPathToDecryp = filesPath;
                    break;
                case "key1Box":
                    key1PathToDecryp = filesPath.First();
                    break;
                case "key2Box":
                    key2PathToDecryp = filesPath.First();
                    break;
            }

            foreach (string file in filesPath)
            {
                listBox.Items.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            filesPathToEncryp = Array.Empty<string>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            filesPathToDecryp = Array.Empty<string>();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            key1Box.Items.Clear();
            key1PathToDecryp = string.Empty;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            key2Box.Items.Clear();
            key2PathToDecryp = string.Empty;
        }

        private void encryptBtn_Click(object sender, EventArgs e)
        {
            if (filesPathToEncryp != null && filesPathToEncryp.Length != 0)
            {
                byte[] myRijndaelKey;
                byte[] myRijndaelIV;

                try
                {
                    string fileName;
                    if (filesPathToEncryp.Length == 1)
                    {
                        fileName = Path.Combine(Path.GetDirectoryName(filesPathToEncryp.First()), Path.GetFileNameWithoutExtension(filesPathToEncryp.First()));
                    }
                    else
                    {
                        fileName = Path.Combine(Path.GetDirectoryName(filesPathToEncryp.First()), "Varios");
                    }

                    using (Rijndael myRijndael = Rijndael.Create())
                    {
                        myRijndaelKey = myRijndael.Key;
                        myRijndaelIV = myRijndael.IV;
                    }

                    foreach (string file in filesPathToEncryp)
                    {
                        using (Rijndael myRijndael = Rijndael.Create())
                        {
                            byte[] encrypted = EncryptStringToBytes(File.ReadAllText(file), myRijndaelKey, myRijndaelIV);

                            File.WriteAllText(file, string.Empty);
                            FileStream fileEncrypt = new FileStream(file, FileMode.Open);
                            fileEncrypt.Write(encrypted);
                            fileEncrypt.Close();
                        }
                    }

                    FileStream fileEncryptKey = new FileStream($"{fileName} - Key-1", FileMode.Create);
                    fileEncryptKey.Write(myRijndaelKey);
                    fileEncryptKey.Close();

                    FileStream fileEncryptIV = new FileStream($"{fileName} - Key-2", FileMode.Create);
                    fileEncryptIV.Write(myRijndaelIV);
                    fileEncryptIV.Close();

                    MessageBox.Show("Encryption Done!", "Success");
                }
                catch
                {
                    MessageBox.Show("Encryption failed!", "Error");
                }
            }
            else
            {
                MessageBox.Show("No files to encrypt!", "Error");
            }

        }

        private void decryptBtn_Click(object sender, EventArgs e)
        {
            using (Rijndael myRijndael = Rijndael.Create())
            {
                foreach (string file in filesPathToDecryp)
                {
                    byte[] cryptFile = File.ReadAllBytes(file);
                    byte[] RijndaelKey = File.ReadAllBytes(key1PathToDecryp);
                    byte[] RijndaelIV = File.ReadAllBytes(key2PathToDecryp);

                    string decrypt = DecryptStringFromBytes(cryptFile, RijndaelKey, RijndaelIV);

                    File.WriteAllText(file, decrypt);
                }
            }

            MessageBox.Show("Decryption done!", "Success");
        }

        static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            using (Rijndael rijAlg = Rijndael.Create())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return encrypted;
        }

        static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string plaintext = null;

            using (Rijndael rijAlg = Rijndael.Create())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}
