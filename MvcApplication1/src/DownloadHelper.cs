using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace MvcApplication1.src
{
    public class DownloadHelper
    {
        public DownloadHelper(string owner)
        {
            OwnerFile = owner;
        }
        public string FileName { set; get; }
        public int FileSize { set; get; }
        public string FileExtension { set; get; }
        public string ErrorMessage { set; get; }
        public string OwnerFile { set; get;  }
        public string UploadUserFile(HttpPostedFileBase file)
        {
            try
            {
                FileSize = file.ContentLength;
                FileName = file.FileName;
                var supportedTypes = new[] { "doc", "pdf" };
                var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                FileExtension = fileExt;
                if (!supportedTypes.Contains(fileExt))
                {
  
                    ErrorMessage = "File Extension Is InValid - Only Upload WORD/PDF/EXCEL/TXT File";
                    return ErrorMessage;
                }
                else if (file.ContentLength > (FileSize * 1024))
                {
                    ErrorMessage = "File size Should Be UpTo " + FileSize + "KB";
                    return ErrorMessage;
                }
                else
                {
                    ErrorMessage = "File Is Successfully Uploaded";
                    string path = GetPathUserDirDoc(file);
                    file.SaveAs(path + "/" + FileName);
                    return ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Upload Container Should Not Be Empty or Contact Admin";
                return ErrorMessage;
            }
        }
        public string GetPathUserDirDoc(HttpPostedFileBase file)
        {
            var path = System.IO.Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data/uploads/Users"), this.OwnerFile);
            if(!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}