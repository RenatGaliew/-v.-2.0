using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Спектральный_анализ_v._2._0
{
    public class WorkingPlace
    {
        public event EventHandler<int> Increment;
        private bool _isReturn = false;
        private bool _analyzDifferenceImage = true;
        private bool _saveDifferenceImage = true;
        public string ResultPath => $"{Path}\\Results\\";
        public string DifferencePath => $"{Path}\\Differences\\";
        public List<string> FilesPaths { get; set; }
        public ObservableCollection<string> FilesNames { get; set; }
        public string Path;
        private List<AccessExcel> AccessesExcel { get; set; }

        public WorkingPlace(string path)
        {
            Path = path;
            FilesPaths = new List<string>();
            FilesNames = new ObservableCollection<string>();
            AccessesExcel = new List<AccessExcel>(4);
            AccessesExcel.Add(new AccessExcel());
            AccessesExcel.Add(new AccessExcel());
            AccessesExcel.Add(new AccessExcel());
            AccessesExcel.Add(new AccessExcel());
            DirectoryInfo ourDir = new DirectoryInfo(Path);
            //просматриваем директорию и отбираем необходимые файлы
            foreach (FileInfo fileName in ourDir.GetFiles())
            {
                //обираем файлы с указанными ниже расширениями 
                if (fileName.Extension == ".png" ||
                    fileName.Extension == ".jpg" ||
                    fileName.Extension == ".bmp" ||
                    fileName.Extension == ".jpeg")
                {
                    //имена файлов записываем в массив
                    FilesPaths.Add(fileName.FullName);
                    FilesNames.Add(fileName.Name);
                }
            }
        }

        public void NewfilePath()
        {
            FolderBrowserDialog folderBrowserDialog1 =
                new FolderBrowserDialog { SelectedPath = Path != "" ? Path : Application.StartupPath };
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                FilesNames?.Clear();
                FilesPaths?.Clear();
                Path = folderBrowserDialog1.SelectedPath;
                //получаем параметры текущей директории
                DirectoryInfo ourDir = new DirectoryInfo(Path);
                //просматриваем директорию и отбираем необходимые файлы
                foreach (FileInfo fileName in ourDir.GetFiles())
                {
                    //обираем файлы с указанными ниже расширениями 
                    if (fileName.Extension == ".png" ||
                        fileName.Extension == ".jpg" ||
                        fileName.Extension == ".bmp" ||
                        fileName.Extension == ".jpeg")
                    {
                        //имена файлов записываем в массив
                        FilesPaths?.Add(fileName.FullName);
                        FilesNames?.Add(fileName.Name);
                    }
                }
            }
        }

        public void Start(int countCube)
        {
            if (!Directory.Exists(ResultPath))
                Directory.CreateDirectory(ResultPath);
            if (!Directory.Exists(DifferencePath))
                Directory.CreateDirectory(DifferencePath);
            AccessesExcel[0].DoAccess($"{ResultPath}Relative.xlsx");
            AccessesExcel[1].DoAccess($"{ResultPath}RelativeDiff.xlsx");
            AccessesExcel[2].DoAccess($"{ResultPath}Absolutly.xlsx");
            AccessesExcel[3].DoAccess($"{ResultPath}AbsolutlyDiff.xlsx");
            double[] lastArray = new double[countCube * countCube * countCube];
            double[] lastArrayInt = new double[countCube * countCube * countCube];
            int i = 0;
            foreach (var fileName in FilesPaths)
            {
                if (i < FilesPaths.Count - 2)
                    Analyzis(fileName, countCube, i, FilesPaths[i++], ref lastArray, ref lastArrayInt,
                        FilesPaths[i + 1]);
                Increment?.Invoke(this,i);
            }
            AccessesExcel[0].FinishAccess();
            AccessesExcel[1].FinishAccess();
            AccessesExcel[2].FinishAccess();
            AccessesExcel[3].FinishAccess();
        }

        private static void SetCountCube(int countCube, Color c, int[,,] cube32)
        {
            int r = c.R * countCube / 256;
            int g = c.G * countCube / 256;
            int b = c.B * countCube / 256;
            cube32[r, g, b]++;
        }

        private void Analyzis(string fileNamePath, int countCube, int indexRow, string fileName, ref double[] lastArray, ref double[] lastArrayInt, string fileNamePath2 = "")
            {
                Image image = Image.FromFile(fileNamePath);
                int width = image.Width;
                int height = image.Height;
                Bitmap bmp = GetBitmap(fileNamePath, countCube, indexRow, fileNamePath2, image, width, height);
                int[,,] cube32 = new int[countCube, countCube, countCube];
                if (width % 2 == 0)
                {
                    for (int i = 0, iLast = width - 1; i < width / 2; i++, iLast--)
                    {
                        for (int j = 0, jLast = height - 1; j < height; j++, jLast--)
                        {
                            Color c = bmp.GetPixel(i, j);
                            SetCountCube(countCube, c, cube32);
                            Color cLast = bmp.GetPixel(iLast, jLast);
                            SetCountCube(countCube, cLast, cube32);
                        }
                    }
                }
                else
                {
                    if (width % 2 != 0)
                    {
                        for (int i = 0, iLast = width - 1; i <= width / 2; i++, iLast--)
                        {
                            if (i != width / 2)
                                for (int j = 0, jLast = height - 1; j < height; j++, jLast--)
                                {
                                    Color c = bmp.GetPixel(i, j);
                                    SetCountCube(countCube, c, cube32);
                                    Color cLast = bmp.GetPixel(iLast, jLast);
                                    SetCountCube(countCube, cLast, cube32);
                                }
                            else
                            {
                                for (int j = 0; j < height; j++)
                                {
                                    Color c = bmp.GetPixel(i, j);
                                    SetCountCube(countCube, c, cube32);
                                }
                            }
                        }
                    }
                }

                SaveTxtAndDataGridView(fileName, countCube, cube32, width, height, ref _isReturn, indexRow, ref lastArray, ref lastArrayInt);
            }
        private static void GetRgbColor(int i, int j, int k, int countCube, out int R, out int G, out int B)
        {
            R = i * 256 / (countCube - 1);
            G = j * 256 / (countCube - 1);
            B = k * 256 / (countCube - 1);
            if (R == 256)
                R = 255;
            if (G == 256)
                G = 255;
            if (B == 256)
                B = 255;
        }
        private void SaveTxtAndDataGridView(string fileName, int countCube, int[,,] cube32, int width, int height, ref bool isReturn, int indexRow, ref double[] lastArray, ref double[] lastArrayInt)
        {
            try
            {
                int index = 1;
                using (StreamWriter sw = new StreamWriter(Path + "\\Result.txt", isReturn, Encoding.Default))
                {
                    for (int i = 0; i < countCube; i++)
                        for (int j = 0; j < countCube; j++)
                            for (int k = 0; k < countCube; k++)
                            {
                                int R, G, B;
                                GetRgbColor(i, j, k, countCube, out R, out G, out B);
                                double nI = (double)cube32[i, j, k] / (width * height);
                                double all = (double)cube32[i, j, k];
                                sw.Write(cube32[i, j, k] + ";");
                                if (lastArray.Length != 0)
                                {
                                    if (indexRow == 0)
                                    {
                                        AccessesExcel[0].SetColorCell(1, index, R, G, B);
                                        AccessesExcel[0].WriteCell(1, index, $"{R}:{G}:{B}");
                                        AccessesExcel[1].SetColorCell(1, index, R, G, B);
                                        AccessesExcel[1].WriteCell(1, index, $"{R}:{G}:{B}");
                                        AccessesExcel[2].WriteCell(1, index, $"{R}:{G}:{B}");
                                        AccessesExcel[2].SetColorCell(1, index, R, G, B);
                                        AccessesExcel[3].SetColorCell(1, index, R, G, B);
                                        AccessesExcel[3].WriteCell(1, index, $"{R}:{G}:{B}");
                                    }
                                    AccessesExcel[0].WriteCell(indexRow + 2, index, nI);
                                    AccessesExcel[1].WriteCell(indexRow + 2, index, nI - lastArrayInt[index - 1]);
                                    AccessesExcel[2].WriteCell(indexRow + 2, index, all);
                                    AccessesExcel[3].WriteCell(indexRow + 2, index, all - lastArray[index - 1]);
                                    lastArray[index - 1] = all;
                                    lastArrayInt[index - 1] = nI;
                                }
                                index++;
                            }

                    sw.WriteLine();
                }

                isReturn = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private Bitmap GetBitmap(string fileNamePath, int countCube, int indexRow, string fileNamePath2, Image image, int width,
            int height)
        {
            Bitmap bmp = _analyzDifferenceImage
                ? GetDifferencePicture(fileNamePath, fileNamePath2, countCube)
                : new Bitmap(image, width, height);
            if (_saveDifferenceImage) bmp.Save($"{DifferencePath}{indexRow}.png", ImageFormat.Png);
            return bmp;
        }
        private Bitmap GetDifferencePicture(string file1, string file2, int countCube)
        {
            var l = 255 / (countCube);
            Image image1 = Image.FromFile(file1);
            Image image2 = Image.FromFile(file2);
            int width = Math.Max(image1.Width, image2.Width);
            int height = Math.Max(image1.Height, image2.Height);
            Bitmap bmp1 = new Bitmap(image1, width, height);
            Bitmap bmp2 = new Bitmap(image2, width, height);
            Bitmap bmp3 = new Bitmap(width, height);
            bmp3.MakeTransparent(Color.FromArgb(0, 0, 0, 0));

            if (width % 2 == 0)
            {
                for (int i = 0, iLast = width - 1; i < width / 2; i++, iLast--)
                {
                    for (int j = 0, jLast = height - 1; j < height; j++, jLast--)
                    {
                        var pixel1 = bmp1.GetPixel(i, j);
                        var pixel2 = bmp2.GetPixel(i, j);
                        var pixelLast1 = bmp1.GetPixel(iLast, jLast);
                        var pixelLast2 = bmp2.GetPixel(iLast, jLast);
                        if (pixel2.R != pixel1.R || pixel2.G != pixel1.G || pixel2.B != pixel1.B)
                            bmp3.SetPixel(i, j, Color.FromArgb(255, bmp2.GetPixel(i, j).R, bmp2.GetPixel(i, j).G, bmp2.GetPixel(i, j).B));
                        if (pixelLast2.R != pixelLast1.R || pixelLast2.G != pixelLast1.G || pixelLast2.B != pixelLast1.B)
                            bmp3.SetPixel(iLast, jLast, Color.FromArgb(255, bmp2.GetPixel(iLast, jLast).R, bmp2.GetPixel(iLast, jLast).G, bmp2.GetPixel(iLast, jLast).B));
                    }
                }
            }
            else
            {
                if (width % 2 != 0)
                {
                    for (int i = 0, iLast = width - 1; i <= width / 2; i++, iLast--)
                    {
                        if (i != width / 2)
                            for (int j = 0, jLast = height - 1; j < height; j++, jLast--)
                            {
                                var pixel1 = bmp1.GetPixel(i, j);
                                var pixel2 = bmp2.GetPixel(i, j);
                                var pixelLast1 = bmp1.GetPixel(iLast, jLast);
                                var pixelLast2 = bmp2.GetPixel(iLast, jLast);
                                if (pixel2.R != pixel1.R || pixel2.G != pixel1.G || pixel2.B != pixel1.B)
                                    bmp3.SetPixel(i, j, Color.FromArgb(255, bmp2.GetPixel(i, j).R, bmp2.GetPixel(i, j).G, bmp2.GetPixel(i, j).B));
                                if (pixelLast2.R != pixelLast1.R || pixelLast2.G != pixelLast1.G || pixelLast2.B != pixelLast1.B)
                                    bmp3.SetPixel(iLast, jLast, Color.FromArgb(255, bmp2.GetPixel(iLast, jLast).R, bmp2.GetPixel(iLast, jLast).G, bmp2.GetPixel(iLast, jLast).B));
                            }
                        else
                        {
                            for (int j = 0; j < height; j++)
                            {
                                var pixel1 = bmp1.GetPixel(i, j);
                                var pixel2 = bmp2.GetPixel(i, j);
                                if (pixel2.R != pixel1.R || pixel2.G != pixel1.G || pixel2.B != pixel1.B)
                                    bmp3.SetPixel(i, j, Color.FromArgb(255, bmp2.GetPixel(i, j).R, bmp2.GetPixel(i, j).G, bmp2.GetPixel(i, j).B));
                            }
                        }
                    }
                }
            }
            bmp1.Dispose();
            bmp2.Dispose();
            return bmp3;
        }
    }
}
