using System;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;
using MonoMac.AppKit;
using BizHawk.Client.EmuHawk;
using System.Windows.Forms;
using System.Reflection;
using BizHawk.Client.Common;

namespace MonoMacWrapper
{
	[MonoMac.Foundation.Register("AppDelegate")]
	public class AppDelegate : NSApplicationDelegate
	{
		private System.Collections.Generic.Dictionary<ToolStripMenuItem, MenuItemAdapter> _menuLookup;
		private NSTimer _masterTimer;
		private MainForm _mainWinForm;
		private Action _queuedAction;
		public AppDelegate(){}

		public override void FinishedLaunching(NSObject notification)
		{
			NSApplication.SharedApplication.BeginInvokeOnMainThread(()=>
			{
				StartApplication();
			});
		}

		public override void WillTerminate (NSNotification notification)
		{
			_mainWinForm.Close();
		}

		private void StartApplication()
		{
			BizHawk.Client.EmuHawk.HawkDialogFactory.OpenDialogClass = typeof(MacOpenFileDialog);
			BizHawk.Client.EmuHawk.HawkDialogFactory.FolderBrowserClass = typeof(MacFolderBrowserDialog);
			Global.Config = ConfigService.Load<Config>(PathManager.DefaultIniPath);
			GlobalWin.GL = new BizHawk.Bizware.BizwareGL.Drivers.OpenTK.IGL_TK();
			GLManager.CreateInstance();
			GlobalWin.GLManager = GLManager.Instance;
			GlobalWin.CR_GL = GlobalWin.GLManager.GetContextForIGL(GlobalWin.GL);

			BizHawk.Common.HawkFile.ArchiveHandlerFactory = new SevenZipSharpArchiveHandler();
			try
			{
				_mainWinForm = new BizHawk.Client.EmuHawk.MainForm(new string[0]);
				var title = _mainWinForm.Text;
				_mainWinForm.Show();
				DoMenuExtraction();
				_mainWinForm.MainMenuStrip.Visible = false; //Hide the real one, since it's been extracted
				_mainWinForm.Text = title;
				_masterTimer = NSTimer.CreateRepeatingTimer(0.00833333333333, MacRunLoop);
				NSRunLoop.Current.AddTimer(_masterTimer, NSRunLoopMode.Common);
			}
			catch (Exception e) 
			{
				NSAlert nsa = new NSAlert();
				nsa.MessageText = e.ToString();
				nsa.RunModal();
			}
		}

		private void MacRunLoop(){
			if (_mainWinForm.RunLoopCore()) {
				if (_queuedAction != null) {
					_queuedAction.Invoke (); //Needs to happen in the same context as the RunLoop, otherwise we'll get weird behavior.
					_queuedAction = null;
				}
			} else {
				_masterTimer.Invalidate();
				NSApplication.SharedApplication.Terminate(this);
			}
		}
				
		private void DoMenuExtraction()
		{
			_menuLookup = new System.Collections.Generic.Dictionary<ToolStripMenuItem, MenuItemAdapter>();
			ExtractMenus(_mainWinForm.MainMenuStrip);
		}
		
		private void ExtractMenus(System.Windows.Forms.MenuStrip menus)
		{
			for(int i=0; i<menus.Items.Count; i++)
			{
				ToolStripMenuItem item = menus.Items[i] as ToolStripMenuItem;
				MenuItemAdapter menuOption = new MenuItemAdapter(item);
				NSMenu dropDown = new NSMenu(CleanMenuString(item.Text));
				menuOption.Submenu = dropDown;
				NSApplication.SharedApplication.MainMenu.AddItem(menuOption);
				_menuLookup.Add(item, menuOption);
				menuOption.Hidden = !item.Visible;
				item.VisibleChanged += HandleItemVisibleChanged;
				menuOption.Enabled = item.Enabled;
				ExtractSubmenu(item.DropDownItems, dropDown, i==0); //Skip last 2 options in first menu, redundant exit option
			}
		}
		
		private void ExtractSubmenu(ToolStripItemCollection subItems, NSMenu destMenu, bool fileMenu)
		{
			int max = subItems.Count;
			if(fileMenu) max-=2;
			for(int i=0; i<max; i++)
			{
				ToolStripItem item = subItems[i];
				if(item is ToolStripMenuItem)
				{
					ToolStripMenuItem menuItem = (ToolStripMenuItem)item;
					MenuItemAdapter translated = new MenuItemAdapter(menuItem);
					menuItem.CheckedChanged += HandleMenuItemCheckedChanged;
					menuItem.EnabledChanged += HandleMenuItemEnabledChanged;
					translated.Action = new MonoMac.ObjCRuntime.Selector("HandleMenu");
					translated.State = menuItem.Checked ? NSCellStateValue.On : NSCellStateValue.Off;
					if(menuItem.Image != null) translated.Image = ImageToCocoa(menuItem.Image);
					destMenu.AddItem(translated);
					_menuLookup.Add(menuItem, translated);
					if(menuItem.DropDownItems.Count > 0)
					{
						NSMenu dropDown = new NSMenu(CleanMenuString(item.Text));
						translated.Submenu = dropDown;
						ExecuteDropDownOpened(menuItem);
						ExtractSubmenu(menuItem.DropDownItems, dropDown, false);
					}
				}
				else if(item is ToolStripSeparator)
				{
					destMenu.AddItem(NSMenuItem.SeparatorItem);
				}
			}
		}
		
		private void ExecuteDropDownOpened(ToolStripMenuItem item)
		{
			var dropDownOpeningKey = typeof(ToolStripDropDownItem).GetField("DropDownOpenedEvent", BindingFlags.Static | BindingFlags.NonPublic);
			var eventProp = typeof(ToolStripDropDownItem).GetProperty("Events", BindingFlags.Instance | BindingFlags.NonPublic);
			if (eventProp != null && dropDownOpeningKey != null)
			{
				var dropDownOpeningValue = dropDownOpeningKey.GetValue(item);
				var eventList = eventProp.GetValue(item, null) as System.ComponentModel.EventHandlerList;
				if(eventList != null)
				{
					Delegate ddd = eventList[dropDownOpeningValue];
					try{
						if(ddd!=null) ddd.DynamicInvoke(null, EventArgs.Empty);
					}
					catch(Exception ex){
						//throw ex;
					}
				}
	        }
		}
		
		private void HandleItemVisibleChanged(object sender, EventArgs e)
		{
			if(sender is ToolStripMenuItem && _menuLookup.ContainsKey((ToolStripMenuItem)sender))
			{
				MenuItemAdapter translated = _menuLookup[(ToolStripMenuItem)sender];
				translated.Hidden = !translated.Hidden; 
				//Can't actually look at Visible property because the entire menubar is hidden.
				//Since the event only gets called when Visible is changed, we can assume it got flipped.
				if(((ToolStripMenuItem)sender).Text.Equals("&NES")){
					//Hack to rebuild menu contents due to changing FDS sub-menu.
					//At some point, I might want to figure out a better way to do this.
					RemoveMenuItems(translated);
					ExtractSubmenu(translated.HostMenu.DropDownItems, translated.Submenu, false);
				}
			}
		}

		private void RemoveMenuItems(MenuItemAdapter menu)
		{
			if(menu.HasSubmenu)
			{
				for(int i=menu.Submenu.Count-1; i>=0; i--)
				{
					MenuItemAdapter item = menu.Submenu.ItemAt(i) as MenuItemAdapter;
					if(item != null) //It will be null if it's a separator
					{
						RemoveMenuItems(item);
						if(_menuLookup.ContainsKey(item.HostMenu))
						{
							_menuLookup.Remove(item.HostMenu);
						}
						item.HostMenu.CheckedChanged -= HandleMenuItemCheckedChanged;
						item.HostMenu.EnabledChanged -= HandleMenuItemEnabledChanged;
					}
					menu.Submenu.RemoveItemAt(i);
				}
			}
		}

		private void HandleMenuItemEnabledChanged(object sender, EventArgs e)
		{
			if(sender is ToolStripMenuItem && _menuLookup.ContainsKey((ToolStripMenuItem)sender))
			{
				MenuItemAdapter translated = _menuLookup[(ToolStripMenuItem)sender];
				translated.Enabled = translated.HostMenu.Enabled;
			}
		}

		private void HandleMenuItemCheckedChanged(object sender, EventArgs e)
		{
			if(sender is ToolStripMenuItem && _menuLookup.ContainsKey((ToolStripMenuItem)sender))
			{
				MenuItemAdapter translated = _menuLookup[(ToolStripMenuItem)sender];
				translated.State = translated.HostMenu.Checked ? NSCellStateValue.On : NSCellStateValue.Off;
			}
		}
		
		private static NSImage ImageToCocoa(System.Drawing.Image input)
		{
			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			input.Save(ms,System.Drawing.Imaging.ImageFormat.Png);
			ms.Position = 0;
			NSImage img = NSImage.FromStream(ms);
			img.Size = new System.Drawing.SizeF(16f, 16f); //Some of BizHawk's menu icons are larger, even though WinForms only does 16x16.
			return img;
		}
		
		private static string CleanMenuString(string text)
		{
			return text.Replace("&",string.Empty);
		}
		
		private class MenuItemAdapter : NSMenuItem
		{
			public MenuItemAdapter(ToolStripMenuItem host) : base(CleanMenuString(host.Text)) 
			{
				HostMenu = host;
			}
			public ToolStripMenuItem HostMenu { get;set; }
		}
		
		[Export("HandleMenu")]
		private void HandleMenu(MenuItemAdapter item)
		{
			_queuedAction = new Action(item.HostMenu.PerformClick);
		}
	}
}