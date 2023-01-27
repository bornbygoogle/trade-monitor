using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace BlazorApp.Shared
{
    public class ClsUtil
    {
        #region File/byte array static Methods

        /// <summary>
        /// convertit un byte array en string puis l'enregistre en fichier
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="nSize"></param>
        /// <param name="sFileDest"></param>
        /// <returns></returns>
        public static bool ByteArrayToFile(byte[] buf, string sFileDest, out string sMsgErr)
        {
            bool bResult = false;

            sMsgErr = "";
            try
            {
                bResult = ByteArrayToFile(buf, buf.Length, sFileDest);
            }
            catch (Exception e)
            {
                sMsgErr = e.Message;
            }

            return bResult;

        }

        public static bool ByteArrayToFileUnzipIfNeeded(byte[] buf, string sFileDest, out string sMsgErr)
        {
            if (IsGZip(buf))
            {
                try
                {
                    return ByteArrayZipToFile(buf, sFileDest, out sMsgErr);
                }
                catch
                { }
            }

            return ByteArrayToFile(buf, sFileDest, out sMsgErr);
        }

        public static bool IsGZip(byte[] array)
        {
            return array.Length >= 3 && array[0] == 31 && array[1] == 139 && array[2] == 8;
        }

        /// <summary>
        /// convertit un byte array en string puis l'enregistre en fichier
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="nSize"></param>
        /// <param name="sFileDest"></param>
        /// <returns></returns>
        public static bool ByteArrayToFile(byte[] buf, int nSize, string sFileDest)
        {
            bool bResult = false;

            FileStream fs = null;
            try
            {
                fs = new FileStream(sFileDest, FileMode.Create);
                fs.Write(buf, 0, nSize);
                fs.Flush();
                bResult = true;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }

            return bResult && File.Exists(sFileDest);
        }

        /// <summary>
        /// Convertit un byte array en string
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="nSize"></param>
        /// <param name="sString"></param>
        /// <returns></returns>
        public static bool ByteArrayToString(byte[] buf, int nSize, Encoding encoding, out string sString)
        {
            bool bResult = false;

            sString = "";
            try
            {
                sString = encoding.GetString(buf);
                bResult = true;
            }
            finally
            {
            }

            return bResult;
        }

        #endregion

        #region File/byte array/Gzip static Methods

        /// <summary>
        /// Convertit un byte array en byte array zippé (format GZip)
        /// </summary>
        /// <param name="aBuf"></param>
        /// <param name="aZip"></param>
        /// <param name="sMsgErr"></param>
        /// <returns></returns>
        public static bool ByteArrayToByteArrayZip(byte[] aBuf, out byte[] aZip, out string sMsgErr)
        {
            bool bResult = false;

            sMsgErr = "";
            aZip = new byte[0];
            try
            {
                // Création du MemoryStream qui va contenir le fichier compressé
                MemoryStream ms = new MemoryStream();

                // Compression des données
                GZipStream gz = new GZipStream(ms, CompressionMode.Compress, true);
                // Écriture des données compressées dans le MemoryStream de destination
                gz.Write(aBuf, 0, aBuf.Length);
                // Fermeture du GZipStream
                gz.Close();
                gz.Dispose();

                //Recopie du résultat contenu du MemoryStream dans aZip
                aZip = ms.ToArray();

                //Fermeture du MemoryStream
                ms.Close();

                bResult = true;
            }
            catch (Exception e)
            { sMsgErr = e.Message; }
            return bResult;
        }

        /// <summary>
        /// Convertit une string en byte array zippé (format GZip) dans l'encoding spécifié
        /// </summary>
        /// <param name="sZip"></param>
        /// <param name="aZip"></param>
        /// <param name="sMsgErr"></param>
        /// <returns></returns>
        public static bool StringToByteArrayZip(string sZip, Encoding encoding, out byte[] aZip, out string sMsgErr)
        {
            bool bResult = false;

            sMsgErr = "";
            aZip = new byte[0];
            try
            {
                // conversion de la string en byte array
                //byte[] aBuf = System.Text.Encoding.UTF8.GetBytes(sZip);
                //Attention, ne pas modifier sinon met en l'air les Xml de la douane => au pire en faire un paramètre
                byte[] aBuf = encoding.GetBytes(sZip);
                bResult = ByteArrayToByteArrayZip(aBuf, out aZip, out sMsgErr);
            }
            catch (Exception e)
            { sMsgErr = e.Message; }
            return bResult;
        }

        /// <summary>
        /// Convertit une string en byte array dans l'encoding spécifié
        /// </summary>
        /// <param name="sZip"></param>
        /// <param name="aZip"></param>
        /// <param name="sMsgErr"></param>
        /// <returns></returns>
        public static bool StringToByteArray(string sString, Encoding encoding, out byte[] aByte, out string sMsgErr)
        {
            bool bResult = false;

            sMsgErr = "";
            aByte = new byte[0];
            try
            {
                // conversion de la string en byte array
                //byte[] aBuf = System.Text.Encoding.UTF8.GetBytes(sZip);
                //Attention, ne pas modifier sinon met en l'air les Xml de la douane => au pire en faire un paramètre
                aByte = encoding.GetBytes(sString);
                bResult = true;
            }
            catch (Exception e)
            { sMsgErr = e.Message; }
            return bResult;
        }

        /// <summary>
        /// convertit le contenu d'un fichier en byte array zippé (format GZip)
        /// </summary>
        /// <param name="sFile"></param>
        /// <param name="aZip"></param>
        /// <param name="sMsgErr"></param>
        /// <returns></returns>
        public static bool FileToByteArrayZip(string sFile, out byte[] aZip, out string sMsgErr)
        {
            bool bResult = false;

            sMsgErr = "";
            aZip = new byte[0];
            try
            {
                if (File.Exists(sFile))
                {
                    // Le fichier est placé dans le buffer
                    byte[] aBuf = File.ReadAllBytes(sFile);
                    bResult = ByteArrayToByteArrayZip(aBuf, out aZip, out sMsgErr);

                }
            }
            catch (Exception e)
            { sMsgErr = e.Message; }
            return bResult;
        }

        /// <summary>
        /// Convertit un byte array zippé (format GZip) en byte array dézippé
        /// </summary>
        /// <param name="aZip"></param>
        /// <param name="aUnZip"></param>
        /// <param name="sMsgErr"></param>
        /// <returns></returns>
        public static bool ByteArrayZipToByteArray(byte[] aZip, out byte[] aUnZip, out string sMsgErr)
        {
            bool bResult = false;
            int nSize = -1;
            byte[] aBuf = new byte[0];

            sMsgErr = "";
            aUnZip = new byte[0];
            try
            {

                MemoryStream msZip = new MemoryStream(aZip);
                GZipStream gzUnzip = new GZipStream(msZip, CompressionMode.Decompress);

                //Test si c'est bien un flux GZip en récupérant le "magic number" de début
                byte[] aMagic = new byte[2];
                msZip.Position = 0;
                msZip.Read(aMagic, 0, 2);

                if (aMagic[0] < 0)
                    throw new IOException("Gzip stream is empty");
                if (aMagic[1] < 0 ||
                   ((aMagic[0] << 8) + aMagic[1]) != 0x1F8B)
                    throw new IOException("not a Gzip format !");

                //Récup de la taille du buffer
                byte[] aSize = new byte[4];
                msZip.Position = msZip.Length - 4;
                msZip.Read(aSize, 0, 4);
                msZip.Position = 0;

                try
                {
                    nSize = BitConverter.ToInt32(aSize, 0);
                    aBuf = new byte[nSize + 100];
                }
                catch
                { }
                if (nSize <= msZip.Length) //taille du flux décomprimé (représenté par les 4 derniers octets)
                    nSize = (int)msZip.Length * 10; //arbitrairement on fixe la taille initiale à x10 la taille comprimée

                int nOffset = 0; int nTotal = 0;
                while (true)
                {
                    int j = gzUnzip.Read(aBuf, nOffset, 100);
                    if (j == 0) break;
                    nOffset += j;
                    nTotal += j;
                }
                gzUnzip.Close();
                gzUnzip.Dispose();
                msZip.Close();
                msZip.Dispose();

                aUnZip = new byte[nTotal];
                Array.ConstrainedCopy(aBuf, 0, aUnZip, 0, nTotal);

                bResult = true;
            }
            catch (Exception e)
            { sMsgErr = e.Message; }
            return bResult;
        }

        /// <summary>
        /// Convertit un tableau de byte zippé (format GZip) en string dézippée puis l'enregiste en fichier
        /// </summary>
        /// <param name="aZip"></param>
        /// <param name="sFile"></param>
        /// <param name="sMsgErr"></param>
        /// <returns></returns>
        public static bool ByteArrayZipToFile(byte[] aZip, string sFile, out string sMsgErr)
        {
            bool bResult = false;
            byte[] aFile = new byte[0];

            sMsgErr = "";
            try
            {
                if (ByteArrayZipToByteArray(aZip, out aFile, out sMsgErr))
                {
                    bResult = ByteArrayToFile(aFile, aFile.Length, sFile);
                }
            }
            catch (Exception e)
            { sMsgErr = e.Message; }
            return bResult;
        }

        public static bool ByteArrayToStringUnzipIfNedeed(byte[] aZip, Encoding encoding, out string sString, out string sMsgErr)
        {
            sMsgErr = null;

            if (IsGZip(aZip))
            {
                try
                {
                    return ByteArrayZipToString(aZip, encoding, out sString, out sMsgErr);
                }
                catch
                { }
            }

            return ByteArrayToString(aZip, aZip.Length, encoding, out sString);
        }

        /// <summary>
        /// Convertit un tableau de byte zippé (format Gzip) en String dézippé
        /// </summary>
        /// <param name="aZip"></param>
        /// <param name="sString"></param>
        /// <param name="sMsgErr"></param>
        /// <returns></returns>
        public static bool ByteArrayZipToString(byte[] aZip, Encoding encoding, out string sString, out string sMsgErr)
        {
            bool bResult = false;
            byte[] aString = new byte[0];

            sString = "";
            sMsgErr = "";
            try
            {
                if (ByteArrayZipToByteArray(aZip, out aString, out sMsgErr))
                    bResult = ByteArrayToString(aString, aString.Length, encoding, out sString);
            }
            catch (Exception e)
            { sMsgErr = e.Message; }
            return bResult;
        }

        /// <summary>
        /// Convertit un fichier en fichier zippé (format Gzip)
        /// </summary>
        /// <param name="sFile"></param>
        /// <param name="sFileZip"></param>
        /// <param name="sMsgErr"></param>
        /// <returns></returns>
        public static bool FileToFileZip(string sFile, string sFileZip, out string sMsgErr)
        {
            bool bResult = false;
            byte[] aBuf;

            sMsgErr = "";
            try
            {
                FileToByteArrayZip(sFile, out aBuf, out sMsgErr);
                ByteArrayToFile(aBuf, aBuf.Length, sFileZip);
            }
            catch
            { }

            return bResult;
        }
        #endregion
    }
}
