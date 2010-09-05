using System;
using System.Collections.Generic;
using System.Text;
using DBDiff.Schema.Model;

namespace DBDiff.Schema.SQLServer.Model
{
    public class FileGroup: SQLServerSchemaBase
    {
        private Boolean readOnly;
        private Boolean defaultFileGroup;
        private FileGroupFiles files;

        public FileGroup(Database parent) : base(StatusEnum.ObjectTypeEnum.FileGroup)
        {
            this.Parent = parent;
            files = new FileGroupFiles(this);
        }

        public FileGroup Clone(Database parent)
        {
            FileGroup file = new FileGroup(parent);
            file.IsDefaultFileGroup = this.IsDefaultFileGroup;
            file.IsReadOnly = this.IsReadOnly;
            file.Name = this.Name;            
            file.Id = this.Id;
            file.Files = this.Files.Clone(file);
            file.Guid = this.Guid;
            return file;
        }

        public FileGroupFiles Files
        {
            get { return files; }
            set { files = value; }
        }

        public Boolean IsDefaultFileGroup
        {
            get { return defaultFileGroup; }
            set { defaultFileGroup = value; }
        }

        public Boolean IsReadOnly
        {
            get { return readOnly; }
            set { readOnly = value; }
        }

        /// <summary>
        /// Compara dos triggers y devuelve true si son iguales, caso contrario, devuelve false.
        /// </summary>
        public static Boolean Compare(FileGroup origen, FileGroup destino)
        {
            if (destino == null) throw new ArgumentNullException("destino");
            if (origen == null) throw new ArgumentNullException("origen");
            if (origen.IsReadOnly != destino.IsReadOnly) return false;
            if (origen.IsDefaultFileGroup != destino.IsDefaultFileGroup) return false;
            return true;
        }

        private string ToSQL(string action)
        {
            string sql = "ALTER DATABASE [" + Parent.Name + "] " + action + " ";
            sql += "FILEGROUP [" + Name + "]";
            if (IsDefaultFileGroup) sql += " DEFAULT";
            if (IsReadOnly) sql += " READONLY";
            sql += "\r\nGO\r\n";
            return sql;
        }

        public string ToSQL()
        {
            string sql = ToSQL("ADD");
            foreach (FileGroupFile file in this.Files)
                sql += file.ToSQL();
            return sql;
        }

        public override string ToSQLAdd()
        {
            string sql = ToSQL("ADD");
            foreach (FileGroupFile file in this.Files)
                sql += file.ToSQLAdd();
            return sql;
        }

        public string ToSQLAlter()
        {
            return ToSQL("MODIFY");
        }

        public override string ToSQLDrop()
        {
            string sql = "";
            sql = Files.ToSQLDrop();
            return sql + "ALTER DATABASE [" + Parent.Name + "] REMOVE FILEGROUP [" + Name + "]\r\nGO\r\n\r\n";
        }

        public SQLScriptList ToSQLDiff()
        {
            SQLScriptList listDiff = new SQLScriptList();

            if (this.Status == StatusEnum.ObjectStatusType.DropStatus)
                listDiff.Add(this.ToSQLDrop(), 1, StatusEnum.ScripActionType.DropFileGroup);
            if (this.Status == StatusEnum.ObjectStatusType.CreateStatus)
                listDiff.Add(this.ToSQLAdd(), 1, StatusEnum.ScripActionType.AddFileGroup);
            if (this.Status == StatusEnum.ObjectStatusType.AlterStatus)
                listDiff.Add(this.ToSQLAlter(), 1, StatusEnum.ScripActionType.AlterFileGroup);

            return listDiff;
        }
    }
}
