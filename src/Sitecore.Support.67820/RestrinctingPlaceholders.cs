using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Form.Core.Configuration;
using Sitecore.Form.Core.Utility;
using Sitecore.Forms.Core.Data;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using Sitecore.WFFM.Abstractions;
using Sitecore.WFFM.Abstractions.Constants.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.Support.Forms.Core.Commands
{
    [Serializable]
    public class RestrinctingPlaceholders : Sitecore.Shell.Framework.Commands.Command
    {
        public override void Execute(Sitecore.Shell.Framework.Commands.CommandContext context)
        {
            Sitecore.Context.ClientPage.Start(this, "Run");
        }

        protected void Run(Sitecore.Web.UI.Sheer.ClientPipelineArgs args)
        {
            Sitecore.Data.Items.Item item = StaticSettings.ContextDatabase.GetItem(IDs.SettingsRoot);
            Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    string value = RestrinctingPlaceholders.GetValue();
                    item.Editing.BeginEdit();
                    item.Fields[FormIDs.ModuleSettingsID].Value = args.Result;
                    item.Editing.EndEdit();
                    this.UpdateAllowedRenderings(value, args.Result);
                    Sitecore.Diagnostics.Log.Audit(this, "Set the following restricting placeholders: {0}", new string[]
					{
						Sitecore.Diagnostics.AuditFormatter.FormatItem(item)
					});
                    return;
                }
            }
            else
            {
                Sitecore.Text.UrlString urlString = new Sitecore.Text.UrlString(Sitecore.UIUtil.GetUri("control:Forms.CustomizeTreeListDialog"));
                Sitecore.Web.UrlHandle urlHandle = new Sitecore.Web.UrlHandle();
                urlHandle["value"] = RestrinctingPlaceholders.GetValue();
                urlHandle["source"] = Sitecore.WFFM.Abstractions.Constants.Core.Constants.RestrinctingPlaceholders;
                urlHandle["language"] = item.Language.Name;
                urlHandle["includetemplatesforselection"] = "Placeholder";
                urlHandle["includetemplatesfordisplay"] = "Placeholder Settings Folder,Placeholder";
                urlHandle["includeitemsfordisplay"] = string.Empty;
                urlHandle["excludetemplatesforselection"] = "Placeholder Settings Folder";
                urlHandle["icon"] = "Business/32x32/table_selection_block.png";
                urlHandle["title"] = DependenciesManager.ResourceManager.Localize("RESTRINCTING_PLACEHOLDERS");
                urlHandle["text"] = DependenciesManager.ResourceManager.Localize("RESTRINCTING_PLACEHOLDERS_TEXT");
                urlHandle.Add(urlString);
                Sitecore.Web.UI.Sheer.SheerResponse.ShowModalDialog(urlString.ToString(), "800px", "500px", string.Empty, true);
                args.WaitForPostBack();
            }
        }

        protected void UpdateAllowedRenderings(string oldValue, string newValue)
        {
            List<string> list = new List<string>(oldValue.Split(new char[]
			{
				'|'
			}));
            List<string> list2 = new List<string>(newValue.Split(new char[]
			{
				'|'
			}));
            foreach (string current in list)
            {
                if (!list2.Contains(current))
                {
                    Sitecore.Data.Items.Item item = StaticSettings.ContextDatabase.GetItem(current);
                    if (item != null)
                    {
                        PlaceholderSettingsDefinition placeholderSettingsDefinition = new PlaceholderSettingsDefinition(item);
                        placeholderSettingsDefinition.RemoveControl(IDs.FormInterpreterID.ToString());
                        placeholderSettingsDefinition.RemoveControl(IDs.FormMvcInterpreterID.ToString());
                    }
                }
            }
            foreach (string current2 in list2)
            {
                if (!list.Contains(current2))
                {
                    Sitecore.Data.Items.Item item2 = StaticSettings.ContextDatabase.GetItem(current2);
                    if (item2 != null)
                    {
                        PlaceholderSettingsDefinition placeholderSettingsDefinition2 = new PlaceholderSettingsDefinition(item2);
                        placeholderSettingsDefinition2.AddControl(IDs.FormInterpreterID.ToString());
                        placeholderSettingsDefinition2.AddControl(IDs.FormMvcInterpreterID.ToString());
                    }
                }
            }
        }
        // Sitecore.Support.67820
        public static Item[] GetPlaceholdersWithAllowedControl(Sitecore.Data.ID controlID)
        {
            Item item = StaticSettings.ContextDatabase.GetItem(ItemIDs.PlaceholderSettingsRoot);
            Assert.IsNotNull(item, "placeholders root");
            string query = string.Format(".//*[contains(@Allowed Controls, '{0}')]", controlID);
            return item.Axes.SelectItems(query);
        }

        private static string GetValue()
        {
            List<Sitecore.Data.Items.Item> list = new List<Sitecore.Data.Items.Item>();
            Sitecore.Data.Items.Item[] placeholdersWithAllowedControl = RestrinctingPlaceholders.GetPlaceholdersWithAllowedControl(IDs.FormInterpreterID);
            Sitecore.Data.Items.Item[] placeholdersWithAllowedControl2 = RestrinctingPlaceholders.GetPlaceholdersWithAllowedControl(IDs.FormMvcInterpreterID);
            if (placeholdersWithAllowedControl != null)
            {
                list = placeholdersWithAllowedControl.ToList<Sitecore.Data.Items.Item>();
            }
            if (placeholdersWithAllowedControl2 != null)
            {
                foreach (Sitecore.Data.Items.Item current in from item in placeholdersWithAllowedControl2
                                                             where list.All((Sitecore.Data.Items.Item x) => x.ID != item.ID)
                                                             select item)
                {
                    list.Add(current);
                }
            }
            return string.Join("|", Sitecore.Form.Core.Utility.Utils.ToStringArray(list.ToArray()));
        }
    }
}