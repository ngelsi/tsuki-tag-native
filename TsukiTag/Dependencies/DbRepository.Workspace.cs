using LiteDB;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models.Repository;

namespace TsukiTag.Dependencies
{
    public interface IWorkspaceDb
    {
        event EventHandler WorkspacesChanged;

        List<Workspace> GetAll();

        Workspace Get(Guid id);

        Workspace GetDefault();

        void AddOrUpdate(List<Workspace> workspaces);
    }

    public partial class DbRepository
    {
        public IWorkspaceDb Workspace{ get; protected set; }

        private class WorkspaceDb : IWorkspaceDb
        {
            public event EventHandler WorkspacesChanged;

            public Workspace Get(Guid id)
            {
                using (var db = new LiteDatabase(MetadataRepositoryPath))
                {
                    return db.GetCollection<Workspace>().FindOne(l => l.Id == id);
                }
            }

            public Workspace GetDefault()
            {
                using (var db = new LiteDatabase(MetadataRepositoryPath))
                {
                    return db.GetCollection<Workspace>().FindOne(l => l.IsDefault);
                }
            }

            public void AddOrUpdate(List<Workspace> workspaces)
            {
                using (var db = new LiteDatabase(MetadataRepositoryPath))
                {
                    var coll = db.GetCollection<Workspace>();
                    var allPreviousWorkspaces = coll.Query().ToList();

                    foreach (var workspace in workspaces)
                    {
                        var dbWorkspace = coll.FindOne(l => l.Id == workspace.Id);
                        if (dbWorkspace == null)
                        {
                            coll.Insert(workspace);
                        }
                        else
                        {
                            dbWorkspace.Name = workspace.Name;
                            dbWorkspace.TagsToAdd = workspace.TagsToAdd;
                            dbWorkspace.TagsToRemove = workspace.TagsToRemove;
                            dbWorkspace.OptionalConditionTags = workspace.OptionalConditionTags;
                            dbWorkspace.MandatoryConditionTags = workspace.MandatoryConditionTags;
                            dbWorkspace.IsDefault = workspace.IsDefault;
                            dbWorkspace.FolderPath = workspace.FolderPath;
                            dbWorkspace.FileNameTemplate = workspace.FileNameTemplate;
                            dbWorkspace.DownloadSourcePictures = workspace.DownloadSourcePictures;
                            dbWorkspace.ConvertToJpg = workspace.ConvertToJpg;
                            dbWorkspace.InjectMetadata = workspace.InjectMetadata;
                            dbWorkspace.InjectTags = workspace.InjectTags;
                            dbWorkspace.DeleteFileOnRemove = workspace.DeleteFileOnRemove;

                            coll.Update(dbWorkspace);
                        }
                    }

                    var deletedWorkspaces = allPreviousWorkspaces.Where(a => !workspaces.Any(l => l.Id == a.Id)).ToList();
                    foreach (var deletedWorkspace in deletedWorkspaces)
                    {
                        coll.Delete(deletedWorkspace.Id);
                        db.GetCollection<WorkspacePicture>().DeleteMany(p => p.ResourceListId == deletedWorkspace.Id);
                    }
                }

                WorkspacesChanged?.Invoke(this, EventArgs.Empty);
            }

            public List<Workspace> GetAll()
            {
                using (var db = new LiteDatabase(MetadataRepositoryPath))
                {
                    var coll = db.GetCollection<Workspace>();
                    var workspaces = coll.Query().ToList();

                    return workspaces.OrderBy(l => l.Name).ToList();
                }
            }
        }
    }
}
