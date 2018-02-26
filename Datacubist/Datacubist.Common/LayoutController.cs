using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DevCore = Datacubist.simplebim.Developer.Core;
using DevCube = Datacubist.simplebim.Developer.Core.DataCube;
using DevDesktop = Datacubist.simplebim.Developer.Desktop;

namespace Datacubist.Common
{
    public class LayoutController
    {
        public void PersistWorkspaceLayout(DevDesktop.Frame.ApplicationWorkspace applicationWorkspace)
        {
            DevDesktop.Workspace.FreeWorkspaceLayout layout = null;
            DevCore.Settings.ApplicationSettings appSettings = default(DevCore.Settings.ApplicationSettings);

            // Bail out if we are resetting the workspace
            //if (_resettingWorkspace)
            //{
            //    return;
            //}

            
            // Get the current layout
            applicationWorkspace.TryGetCurrentLayout(ref layout);
            layout.ReIndex();

            //Don't persist if the layout is empty

            if (layout.Areas.Count > 0)
            {
                // Get the application settings
                appSettings = DevCore.Application.Settings;

                DevCore.Settings.SettingsNode workspacesNode = default(DevCore.Settings.SettingsNode);
                workspacesNode = appSettings.Nodes.GetOrAdd("Workspaces");
                workspacesNode.Location = "Workspaces\\User.index";

                DevCore.Settings.SettingsNode workspaceNode = default(DevCore.Settings.SettingsNode);
                workspaceNode = workspacesNode.Nodes.GetOrAdd(layout.ModuleGuid.ToString());
                workspaceNode.Location = string.Format("User.{0}", layout.ModuleName.ToLower());

                workspaceNode.SetOrAddValue("WorkspaceName", layout.ModuleName);

                DevCore.Settings.SettingsNode namedLayoutNode = default(DevCore.Settings.SettingsNode);
                if (layout.HasLayoutName)
                {
                    namedLayoutNode = workspaceNode.Nodes.GetOrAdd(layout.LayoutName);
                }
                else
                {
                    namedLayoutNode = workspaceNode.Nodes.GetOrAdd("Default");
                }

                // Clear any previous definitions
                namedLayoutNode.Values.Clear();
                namedLayoutNode.Nodes.Clear();

                int areaCounter = 0;
                int groupCounter = 0;

                foreach (DevDesktop.Workspace.FreeWorkspaceArea area in layout.Areas)
                {
                    DevCore.Settings.SettingsNode areaNode = default(DevCore.Settings.SettingsNode);

                    if (area.IsCanvasArea)
                    {
                        areaNode = namedLayoutNode.GetOrAddNode("area:canvas");
                    }
                    else
                    {
                        areaCounter += 1;
                        areaNode = namedLayoutNode.GetOrAddNode("area:" + areaCounter);
                        areaNode.SetOrAddValue("RelativeExtent", area.InitialExtents.RelativeExtent);
                    }

                    areaNode.SetOrAddValue("Location", area.Location.ToString());
                    areaNode.SetOrAddValue("IsStacked", area.IsStacked);

                    // Add the panels
                    foreach (DevDesktop.Workspace.WorkspacePanelBase panel in area.Panels)
                    {
                        this.AddWorkspaceLayoutPanel(panel, areaNode, area.Panels.Count, ref groupCounter);
                    }

                }

            }

        }

        public void AddWorkspaceLayoutPanel(DevDesktop.Workspace.WorkspacePanelBase panel, DevCore.Settings.SettingsNode parentNode, int parentPanelCount, ref int groupCounter)
        {
            DevDesktop.Workspace.WorkspacePalettePanel palettePanel = default(DevDesktop.Workspace.WorkspacePalettePanel);
            DevDesktop.Workspace.WorkspaceLayoutGroup layoutGroup = default(DevDesktop.Workspace.WorkspaceLayoutGroup);

            if (panel is DevDesktop.Workspace.WorkspaceLayoutGroup)
            {
                layoutGroup = (DevDesktop.Workspace.WorkspaceLayoutGroup)panel;
                groupCounter += 1;

                DevCore.Settings.SettingsNode groupNode = default(DevCore.Settings.SettingsNode);
                groupNode = parentNode.GetOrAddNode(string.Format("group:{0}", groupCounter));

                groupNode.SetOrAddValue("IsTabbed", layoutGroup.IsTabbed);
                groupNode.SetOrAddValue("RelativeExtent", panel.InitialExtents.RelativeExtent);


                foreach (DevDesktop.Workspace.WorkspacePanelBase groupPanel in layoutGroup.Panels)
                {

                    if (groupPanel is DevDesktop.Workspace.WorkspaceLayoutGroup)
                    {
                        this.AddWorkspaceLayoutPanel(groupPanel, groupNode, layoutGroup.Panels.Count, ref groupCounter);

                    }
                    else if (groupPanel is DevDesktop.Workspace.WorkspacePalettePanel)
                    {
                        palettePanel = (DevDesktop.Workspace.WorkspacePalettePanel)groupPanel;

                        DevCore.Settings.SettingsNode panelNode = default(DevCore.Settings.SettingsNode);

                        if (palettePanel.Index == 0)
                        {
                            panelNode = groupNode.GetOrAddNode("palette:canvas");
                        }
                        else
                        {
                            panelNode = groupNode.GetOrAddNode(string.Format("palette:{0}", palettePanel.Index));
                        }

                        
                        WriteConfigurationToSettings(panelNode, palettePanel.PaletteConfiguration);

                        panelNode.SetOrAddValue("PaletteGuid", palettePanel.PaletteGuid);

                        if (palettePanel.HasGlobalName)
                        {
                            panelNode.SetOrAddValue("GlobalName", palettePanel.GlobalName);
                        }

                        if (layoutGroup.Panels.Count > 1)
                        {
                            panelNode.SetOrAddValue("RelativeExtent", groupPanel.InitialExtents.RelativeExtent);
                        }

                    }

                }

            }
            else if (panel is DevDesktop.Workspace.WorkspacePalettePanel)
            {
                palettePanel = (DevDesktop.Workspace.WorkspacePalettePanel)panel;

                DevCore.Settings.SettingsNode panelNode = default(DevCore.Settings.SettingsNode);

                if (((DevDesktop.Workspace.WorkspacePalettePanel)panel).Index == 0)
                {
                    panelNode = parentNode.GetOrAddNode("palette:canvas");
                }
                else
                {
                    panelNode = parentNode.GetOrAddNode(string.Format("palette:{0}", palettePanel.Index));
                }

                WriteConfigurationToSettings(panelNode, palettePanel.PaletteConfiguration);

                panelNode.SetOrAddValue("PaletteGuid", palettePanel.PaletteGuid);

                if (palettePanel.HasGlobalName)
                {
                    panelNode.SetOrAddValue("GlobalName", palettePanel.GlobalName);
                }

                if (parentPanelCount > 1)
                {
                    panelNode.SetOrAddValue("RelativeExtent", panel.InitialExtents.RelativeExtent);
                }

            }

        }

        public void WriteConfigurationToSettings(DevCore.Settings.SettingsNode parentNode, DevCore.ModuleConfiguration configuration)
        {
            DevCore.Settings.SettingsNode configurationNode = default(DevCore.Settings.SettingsNode);

            // Create the node for the configuration
            configurationNode = parentNode.GetOrAddNode("Configuration");

            // Clear any previous data
            configurationNode.Nodes.Clear();
            configurationNode.Values.Clear();


            if (configuration.Count == 0)
            {
                // Remove the node
                parentNode.RemoveNode(configurationNode.Key);

            }
            else
            {
                // Add the items from the configuration
                foreach (DevCore.ModuleConfigurationItem item in configuration)
                {
                    configurationNode.SetOrAddValue(item.Key, item.Value);
                }
            }

        }

        public DevCore.ModuleConfiguration GetConfigurationFromSettings(DevCore.Settings.SettingsNode parentNode)
        {
            DevCore.ModuleConfiguration result = default(DevCore.ModuleConfiguration);
            DevCore.Settings.SettingsNode configurationNode = null;

            // Create the empty result
            result = new DevCore.ModuleConfiguration();


            if (parentNode.TryGetNode("Configuration", ref configurationNode))
            {

                foreach (DevCore.Settings.SettingsValueBase value in configurationNode.Values)
                {
                    if (value is DevCore.Settings.SettingsStringValue)
                    {
                        result.Add(value.Name, ((DevCore.Settings.SettingsStringValue)value).Value);
                    }

                }
            }

            return result;

        }

        public DevDesktop.Workspace.WorkspacePanelBase AddPanelToWorkspace(DevCore.Settings.SettingsNode panelNode)
        {

            DevDesktop.Workspace.WorkspaceLayoutGroup layoutGroup = default(DevDesktop.Workspace.WorkspaceLayoutGroup);
            DevDesktop.Workspace.WorkspacePanelBase layoutPanel = default(DevDesktop.Workspace.WorkspacePanelBase);
            DevDesktop.Workspace.WorkspacePalettePanel layoutPalette = default(DevDesktop.Workspace.WorkspacePalettePanel);
            DevCore.Settings.SettingsGuidValue paletteGuidValue = null;


            if (panelNode.Key.StartsWith("group:"))
            {
                // Create the layout group
                layoutGroup = new DevDesktop.Workspace.WorkspaceLayoutGroup(panelNode.GetIntegerValue("RelativeExtent", 100).Value);

                layoutGroup.IsTabbed = panelNode.GetBooleanValue("IsTabbed", false).Value;


                foreach (DevCore.Settings.SettingsNode groupPanelNode in panelNode.Nodes)
                {
                    layoutPanel = this.AddPanelToWorkspace(groupPanelNode);

                    if (layoutPanel != null)
                    {
                        layoutGroup.Panels.Add(layoutPanel);
                    }
                }

                // Return the group
                return layoutGroup;


            }
            else
            {

                if (panelNode.TryGetGuidValue("PaletteGuid", ref paletteGuidValue))
                {
                    layoutPalette = new DevDesktop.Workspace.WorkspacePalettePanel(panelNode.GetIntegerValue("RelativeExtent", 100).Value, paletteGuidValue.Value, panelNode.GetStringValue("GlobalName", string.Empty).Value);

                    GetConfigurationFromSettings(panelNode).Copy(layoutPalette.PaletteConfiguration);

                    return layoutPalette;

                }
                else
                {
                    return null;
                }

            }

        }

        public DevDesktop.Workspace.FreeWorkspaceLayout GetWorkspaceLayout(Guid workspaceGuid, string workspaceLayoutName)
        {
            DevDesktop.Workspace.FreeWorkspaceLayout layout = null;
            DevCore.Settings.SettingsGuidValue paletteGuidValue = null;
            DevCore.Settings.SettingsNode workspaceNode = null;
            DevCore.Settings.ApplicationSettings appSettings = default(DevCore.Settings.ApplicationSettings);
            bool isCanvas = false;
            bool isStacked = false;

            appSettings = DevCore.Application.Settings;
            //// Resolve the layout name
            //if (!e.WorkspaceConfiguration.TryGetValue("LAYOUTNAME", e.WorkspaceLayoutName))
            //{
            //    e.WorkspaceLayoutName = "Default";
            //}

            // Resolve the stored layout

            if (appSettings.TryGetNode(string.Format("Workspaces\\{0}\\{1}", workspaceGuid, workspaceLayoutName), ref workspaceNode))
            {

                if (workspaceNode.Nodes.Count > 0)
                {
                    layout = new DevDesktop.Workspace.FreeWorkspaceLayout(workspaceGuid, workspaceNode.GetStringValue("WorkspaceName", string.Empty).Value);


                    foreach (DevCore.Settings.SettingsNode areaNode in workspaceNode.Nodes)
                    {
                        DevDesktop.Workspace.FreeWorkspaceArea layoutArea = default(DevDesktop.Workspace.FreeWorkspaceArea);
                        DevDesktop.Workspace.WorkspacePanelBase layoutPanel = default(DevDesktop.Workspace.WorkspacePanelBase);
                        DevDesktop.Workspace.WorkspaceComponentExtents extents = default(DevDesktop.Workspace.WorkspaceComponentExtents);
                        DevDesktop.Workspace.WorkspaceAreaLocation location = default(DevDesktop.Workspace.WorkspaceAreaLocation);

                        // Do we have the canvas area?
                        if (areaNode.Key == "area:canvas")
                        {
                            isCanvas = true;
                        }
                        else
                        {
                            isCanvas = false;
                        }

                        // Do we have a stacked area
                        isStacked = areaNode.GetBooleanValue("IsStacked", false).Value;

                        // Resolve the extents
                        if (isCanvas)
                        {
                            extents = new DevDesktop.Workspace.WorkspaceComponentExtents(0);
                        }
                        else
                        {
                            extents = new DevDesktop.Workspace.WorkspaceComponentExtents(areaNode.GetIntegerValue("RelativeExtent", 100).Value);
                        }

                        // Resolve the location
                        switch (areaNode.GetStringValue("Location", string.Empty).Value)
                        {
                            case "Left":
                                location = DevDesktop.Workspace.WorkspaceAreaLocation.Left;
                                break;
                            case "Right":
                                location = DevDesktop.Workspace.WorkspaceAreaLocation.Right;
                                break;
                            case "Bottom":
                                location = DevDesktop.Workspace.WorkspaceAreaLocation.Bottom;
                                break;
                            case "Top":
                                location = DevDesktop.Workspace.WorkspaceAreaLocation.Top;
                                break;
                            default:
                                location = DevDesktop.Workspace.WorkspaceAreaLocation.Undefined;
                                break;
                        }

                        layoutArea = new DevDesktop.Workspace.FreeWorkspaceArea(extents, location);
                        layout.AddArea(layoutArea, isCanvas, isStacked);

                        // Add the panels to the area

                        foreach (DevCore.Settings.SettingsNode panelNode in areaNode.Nodes)
                        {
                            layoutPanel = this.AddPanelToWorkspace(panelNode);

                            if (layoutPanel != null)
                            {
                                layoutArea.Panels.Add(layoutPanel);
                            }
                        }
                    }

                }

            }
            return layout;
        }
    }
}
