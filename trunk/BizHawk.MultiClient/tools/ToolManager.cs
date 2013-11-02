﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BizHawk.MultiClient
{
	public class ToolManager
	{
		//TODO: merge ToolHelper code where logical
		//For instance, add an IToolForm property called UsesCheats, so that a UpdateCheatRelatedTools() method can update all tools of this type
		//Also a UsesRam, and similar method

		private List<IToolForm> _tools = new List<IToolForm>();

		/// <summary>
		/// Loads the tool dialog T, if it does not exist it will be created, if it is already open, it will be focused
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IToolForm Load<T>() where T : IToolForm
		{
			var existingTool = _tools.FirstOrDefault(x => x is T);
			if (existingTool != null)
			{
				if (existingTool.IsDisposed)
				{
					_tools.Remove(existingTool);
				}
				else
				{
					existingTool.Show();
					existingTool.Focus();
					return existingTool;
				}
			}

			var result = Get<T>();
			result.Show();
			return result;
		}

		/// <summary>
		/// Returns true if an instance of T exists
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public bool Has<T>() where T : IToolForm
		{
			return _tools.Any(x => x is T);
		}

		/// <summary>
		/// Gets the instance of T, or creates and returns a new instance
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IToolForm Get<T>() where T : IToolForm
		{
			var existingTool = _tools.FirstOrDefault(x => x is T);
			if (existingTool != null)
			{
				return existingTool;
			}
			else
			{
				var tool = Activator.CreateInstance(typeof(T));

				//Add to the list and extract it, so it will be strongly typed as T
				_tools.Add(tool as IToolForm);
				return _tools.FirstOrDefault(x => x is T);
			}
			
		}

		public void UpdateBefore()
		{
			var beforeList = _tools.Where(x => x.UpdateBefore);
			foreach (var tool in beforeList)
			{
				tool.UpdateValues();
			}
		}

		public void UpdateAfter()
		{
			var afterList = _tools.Where(x => !x.UpdateBefore);
			foreach (var tool in afterList)
			{
				tool.UpdateValues();
			}
		}

		/// <summary>
		/// Calls UpdateValues() on an instance of T, if it exists
		/// </summary>
		public void UpdateValues<T>() where T : IToolForm
		{
			var tool = _tools.FirstOrDefault(x => x is T);
			if (tool != null)
			{
				tool.UpdateValues();
			}
		}

		public void Restart()
		{
			_tools.ForEach(x => x.Restart());
		}

		/// <summary>
		/// Calls Restart() on an instance of T, if it exists
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Restart<T>() where T : IToolForm
		{
			var tool = _tools.FirstOrDefault(x => x is T);
			if (tool != null)
			{
				tool.Restart();
			}
		}

		/// <summary>
		/// Runs AskSave on every tool dialog, false is returned if any tool returns false
		/// </summary>
		/// <returns></returns>
		public bool AskSave()
		{
			foreach (var tool in _tools)
			{
				var result = tool.AskSave();
				if (!result)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Calls AskSave() on an instance of T, if it exists, else returns true
		/// The caller should interpret false as cancel and will back out of the action that invokes this call
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public bool AskSave<T>() where T : IToolForm
		{
			var tool = _tools.FirstOrDefault(x => x is T);
			if (tool != null)
			{
				return tool.AskSave();
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// If T exists, this call will close the tool, and remove it from memory
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Close<T>() where T : IToolForm
		{
			var tool = _tools.FirstOrDefault(x => x is T);
			if (tool != null)
			{
				tool.Close();
				_tools.Remove(tool);
			}
		}

		public void Close()
		{
			_tools.ForEach(x => x.Close());
			_tools.Clear();
		}

		//Note: Referencing these properties creates an instance of the tool and persists it.  They should be referenced by type if this is not desired
		#region Tools

		public RamWatch RamWatch
		{
			get
			{
				var tool = _tools.FirstOrDefault(x => x is RamWatch);
				if (tool != null)
				{
					if (tool.IsDisposed)
					{
						_tools.Remove(tool);
					}
					else
					{
						return tool as RamWatch;
					}
				}
				
				var newTool = new RamWatch();
				_tools.Add(newTool);
				return newTool;
			}
		}

		public RamSearch RamSearch
		{
			get
			{
				var tool = _tools.FirstOrDefault(x => x is RamSearch);
				if (tool != null)
				{
					if (tool.IsDisposed)
					{
						_tools.Remove(tool);
					}
					else
					{
						return tool as RamSearch;
					}
				}

				var newTool = new RamSearch();
				_tools.Add(newTool);
				return newTool;
			}
		}

		public HexEditor HexEditor
		{
			get
			{
				var tool = _tools.FirstOrDefault(x => x is HexEditor);
				if (tool != null)
				{
					if (tool.IsDisposed)
					{
						_tools.Remove(tool);
					}
					else
					{
						return tool as HexEditor;
					}
				}

				var newTool = new HexEditor();
				_tools.Add(newTool);
				return newTool;
			}
		}

		public VirtualPadForm VirtualPad
		{
			get
			{
				var tool = _tools.FirstOrDefault(x => x is VirtualPadForm);
				if (tool != null)
				{
					if (tool.IsDisposed)
					{
						_tools.Remove(tool);
					}
					else
					{
						return tool as VirtualPadForm;
					}
				}

				var newTool = new VirtualPadForm();
				_tools.Add(newTool);
				return newTool;
			}
		}

		public SNESGraphicsDebugger SNESGraphicsDebugger
		{
			get
			{
				var tool = _tools.FirstOrDefault(x => x is SNESGraphicsDebugger);
				if (tool != null)
				{
					if (tool.IsDisposed)
					{
						_tools.Remove(tool);
					}
					else
					{
						return tool as SNESGraphicsDebugger;
					}
				}

				var newTool = new SNESGraphicsDebugger();
				_tools.Add(newTool);
				return newTool;
			}
		}

		#endregion
	}
}
