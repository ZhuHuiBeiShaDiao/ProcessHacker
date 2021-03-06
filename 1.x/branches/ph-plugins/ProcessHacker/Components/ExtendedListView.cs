﻿/*
 * Process Hacker - 
 *   ProcessHacker Extended ListView 
 * 
 * Copyright (C) 2009 dmex
 * 
 * This file is part of Process Hacker.
 * 
 * Process Hacker is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Process Hacker is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Process Hacker.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */


using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ProcessHacker.Native;
using ProcessHacker.Native.Api;

namespace ProcessHacker
{
    public class ExtendedListView : ListView
    {
        private const int LVM_First = 0x1000;                              // ListView messages
        private const int LVM_SetGroupInfo = (LVM_First + 147);            // ListView messages Setinfo on Group
        private const int LVM_SetExtendedListViewStyle = (LVM_First + 54); // Sets extended styles in list-view controls. 
        private const int LVS_Ex_DoubleBuffer = 0x00010000;                // Paints via double-buffering, which reduces flicker. also enables alpha-blended marquee selection.

        private const int LVN_First = -100;
        private const int LVN_LINKCLICK = (LVN_First - 84);

        private delegate void CallBackSetGroupState(ListViewGroup lvGroup, ListViewGroupState lvState, string task);
        private delegate void CallbackSetGroupString(ListViewGroup lvGroup, string value);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, ref LVGroup lParam);

        public ExtendedListView()
        {
            //Activate double buffering and
            //Enable the OnNotifyMessage event so we get a chance to filter out 
            //Windows messages before they get to the form's WndProc
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.EnableNotifyMessage, true);
        }

        public void SetGroupState(ListViewGroupState state)
        {
            this.SetGroupState(state, null);
        }

        public void SetGroupState(ListViewGroupState state, string taskLabel)
        {
            foreach (ListViewGroup lvg in this.Groups)
            {
                SetGrpState(lvg, state, taskLabel);
            }
        }

        private static int? GetGroupID(ListViewGroup lvGroup)
        {
            int? grpId = null;
            Type grpType = lvGroup.GetType();
            if (grpType != null)
            {
                PropertyInfo pInfo = grpType.GetProperty("ID", BindingFlags.NonPublic | BindingFlags.Instance);
                if (pInfo != null)
                {
                    object tmprtnval = pInfo.GetValue(lvGroup, null);
                    if (tmprtnval != null)
                    {
                        grpId = tmprtnval as int?;
                    }
                }
            }
            return grpId;
        }

        private void SetGrpState(ListViewGroup lvGroup, ListViewGroupState grpState, string task)
        {
            if (OSVersion.IsBelow(WindowsVersion.Vista))
                return;
            if (lvGroup == null || lvGroup.ListView == null)
                return;
            if (lvGroup.ListView.InvokeRequired)
                lvGroup.ListView.Invoke(new CallBackSetGroupState(SetGrpState), lvGroup, grpState, task);
            else
            {
                int? GrpId = GetGroupID(lvGroup);
                int gIndex = lvGroup.ListView.Groups.IndexOf(lvGroup);
                LVGroup group = new LVGroup();
                group.CbSize = Marshal.SizeOf(group);
                group.Mask |= ListViewGroupMask.Task 
                    | ListViewGroupMask.State 
                    | ListViewGroupMask.Align;

                IntPtr taskString = Marshal.StringToHGlobalAuto(task);

                if (task.Length > 1)
                {
                    group.Task = taskString;
                    group.CchTask = task.Length;
                }

                group.GroupState = grpState;
          
                if (GrpId != null)
                {
                    group.GroupId = GrpId.Value;
                    SendMessage(base.Handle, LVM_SetGroupInfo, GrpId.Value, ref group);
                }
                else
                {
                    group.GroupId = gIndex;
                    SendMessage(base.Handle, LVM_SetGroupInfo, gIndex, ref group);
                }
                lvGroup.ListView.Refresh();

                Marshal.FreeHGlobal(taskString);
            }
        }

        protected override void OnNotifyMessage(Message m)
        {
            //notification for linkclick never reaches here?
            //http://msdn.microsoft.com/en-us/library/bb774851%28VS.85%29.aspx

            //

            //Filter out the WM_ERASEBKGND message and prevent any type of flickering
            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }
        }

        private const int WM_NOTIFY = 0x004E;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x1: /*WM_CREATE*/
                    {
                        SubclassHWnd(base.Handle);
             
                        HResult setThemeResult = Win32.SetWindowTheme(base.Handle, "explorer", null);
                        setThemeResult.ThrowIf();

                        unchecked
                        {
                            Win32.SendMessage(base.Handle, (WindowMessage)LVM_SetExtendedListViewStyle, LVS_Ex_DoubleBuffer, LVS_Ex_DoubleBuffer);
                        }

                        break;
                    }  
                case 0x202:
                case 0x205:
                case 520:
                case 0x203:
                case 0x2a1:
                    {
                        base.DefWndProc(ref m);
                        return;
                    }
            }

            base.WndProc(ref m);
        }

        // Win32 API needed
        [DllImport("user32")]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, Win32WndProc newProc);
        [DllImport("user32")]
        private static extern int CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int Msg, int wParam, int lParam);

        // A delegate that matches Win32 WNDPROC:
        private delegate int Win32WndProc(IntPtr hWnd, int Msg, int wParam, int lParam);

        // from winuser.h:
        private const int GWL_WNDPROC = -4;
        private const int WM_LBUTTONDOWN = 0x0201;

        // program variables
        private IntPtr oldWndProc = IntPtr.Zero;
        private Win32WndProc newWndProc = null;

        void SubclassHWnd(IntPtr hWnd)
        {
            // hWnd is the window we want to subclass..., create a new delegate for the new wndproc
            newWndProc = new Win32WndProc(MyWndProc);
            // subclass
            oldWndProc = SetWindowLong(hWnd, GWL_WNDPROC, newWndProc);
        }

        // this is the new wndproc, just show a messagebox on left button down:
        private int MyWndProc(IntPtr hWnd, int Msg, int wParam, int lParam)
        {
            System.Diagnostics.Debug.WriteLine(Msg.ToString());


            switch (Msg)
            {
                case 0x4e:
                    {
                        unsafe
                        {
                            NMHDR* hdr = (NMHDR*)lParam;

                            if (hdr->code == LVN_LINKCLICK)
                            {
                                MessageBox.Show("Link clicked!");
                                return 0;
                            }
                        }
                        break;
                    }
                default:
                    break;
            }

            return CallWindowProc(oldWndProc, hWnd, Msg, wParam, lParam);
        }


        //http://msdn.microsoft.com/en-us/library/bb774769(VS.85).aspx
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct LVGroup
        {
            /// <summary>
            /// Size of this structure, in bytes.
            /// </summary>
            public int CbSize;

            /// <summary>
            /// Mask that specifies which members of the structure are valid input. One or more of the following values:LVGF_NONENo other items are valid.
            /// </summary>
            public ListViewGroupMask Mask;

            /// <summary>
            /// Pointer to a null-terminated string that contains the header text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the header text.
            /// </summary>
            //[MarshalAs(UnmanagedType.LPWStr)]
            public IntPtr pszHeader;

            /// <summary>
            /// Size in TCHARs of the buffer pointed to by the pszHeader member. If the structure is not receiving information about a group, this member is ignored.
            /// </summary>
            public int CchHeader;

            /// <summary>
            /// Pointer to a null-terminated string that contains the footer text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the footer text.
            /// </summary>
            //[MarshalAs(UnmanagedType.LPWStr)]
            public IntPtr pszFooter;

            /// <summary>
            /// Size in TCHARs of the buffer pointed to by the pszFooter member. If the structure is not receiving information about a group, this member is ignored.
            /// </summary>
            public int CchFooter;

            /// <summary>
            /// ID of the group.
            /// </summary>
            public int GroupId;

            /// <summary>
            /// Mask used with LVM_GETGROUPINFO (Microsoft Windows XP and Windows Vista) and LVM_SETGROUPINFO (Windows Vista only) to specify which flags in the state value are being retrieved or set.
            /// </summary>
            public uint stateMask;

            /// <summary>
            /// Flag that can have one of the following values:LVGS_NORMALGroups are expanded, the group name is displayed, and all items in the group are displayed.
            /// </summary>
            public ListViewGroupState GroupState;

            /// <summary>
            /// Indicates the alignment of the header or footer text for the group. It can have one or more of the following values. Use one of the header flags. Footer flags are optional. Windows XP: Footer flags are reserved.LVGA_FOOTER_CENTERReserved.
            /// </summary>
            public uint uAlign;

            /// <summary>
            /// Windows Vista. Pointer to a null-terminated string that contains the subtitle text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the subtitle text. This element is drawn under the header text.
            /// </summary>
            //[MarshalAs(UnmanagedType.LPWStr)]
            public IntPtr PszSubtitle;

            /// <summary>
            /// Windows Vista. Size, in TCHARs, of the buffer pointed to by the pszSubtitle member. If the structure is not receiving information about a group, this member is ignored.
            /// </summary>
            public uint CchSubtitle;

            /// <summary>
            /// Windows Vista. Pointer to a null-terminated string that contains the text for a task link when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the task text. This item is drawn right-aligned opposite the header text. When clicked by the user, the task link generates an LVN_LINKCLICK notification.
            /// </summary>
            //[MarshalAs(UnmanagedType.LPWStr)]
            public IntPtr Task;

            /// <summary>
            /// Windows Vista. Size in TCHARs of the buffer pointed to by the pszTask member. If the structure is not receiving information about a group, this member is ignored.
            /// </summary>
            public int CchTask;

            /// <summary>
            /// Windows Vista. Pointer to a null-terminated string that contains the top description text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the top description text. This item is drawn opposite the title image when there is a title image, no extended image, and uAlign==LVGA_HEADER_CENTER.
            /// </summary>
            //[MarshalAs(UnmanagedType.LPWStr)]
            public IntPtr DescriptionTop;

            /// <summary>
            /// Windows Vista. Size in TCHARs of the buffer pointed to by the pszDescriptionTop member. If the structure is not receiving information about a group, this member is ignored.
            /// </summary>
            public uint CchDescriptionTop;

            /// <summary>
            /// Windows Vista. Pointer to a null-terminated string that contains the bottom description text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the bottom description text. This item is drawn under the top description text when there is a title image, no extended image, and uAlign==LVGA_HEADER_CENTER.
            /// </summary>
            //[MarshalAs(UnmanagedType.LPWStr)]
            public IntPtr DescriptionBottom;

            /// <summary>
            /// Windows Vista. Size in TCHARs of the buffer pointed to by the pszDescriptionBottom member. If the structure is not receiving information about a group, this member is ignored.
            /// </summary>
            public uint CchDescriptionBottom;

            /// <summary>
            /// Windows Vista. Index of the title image in the control imagelist.
            /// </summary>
            public int ITitleImage;

            /// <summary>
            /// Windows Vista. Index of the extended image in the control imagelist.
            /// </summary>
            public int IExtendedImage;

            /// <summary>
            /// Windows Vista. Read-only.
            /// </summary>
            public int IFirstItem;

            /// <summary>
            /// Windows Vista. Read-only in non-owner data mode.
            /// </summary>
            public uint CItems;

            /// <summary>
            /// Windows Vista. NULL if group is not a subset. Pointer to a null-terminated string that contains the subset title text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the subset title text.
            /// </summary>
            //[MarshalAs(UnmanagedType.LPWStr)]
            public IntPtr PszSubsetTitle;

            /// <summary>
            /// Windows Vista. Size in TCHARs of the buffer pointed to by the pszSubsetTitle member. If the structure is not receiving information about a group, this member is ignored.
            /// </summary>
            public uint CchSubsetTitle;
        }

        //http://msdn.microsoft.com/en-us/library/ms229669.aspx
        //http://msdn.microsoft.com/en-us/magazine/dvdarchive/cc163384.aspx       
        /// <summary>
        /// WM_NOTIFY notificaiton message header.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NMHDR
        {
            /// <summary>
            /// Window handle to the control sending a message.
            /// </summary>
            public IntPtr hwndFrom;
            /// <summary>
            /// Identifier of the control sending a message.
            /// </summary>
            public IntPtr idFrom;
            /// <summary>
            /// Notification code. This member can be a control-specific notification code or it can be one of the common notification codes.
            /// </summary>
            public int code;
        }

    }

    [Flags]
    public enum ListViewGroupMask : uint
    {
        None = 0x00000,
        Header = 0x00001,
        Footer = 0x00002,
        State = 0x00004,
        Align = 0x00008,
        GroupId = 0x00010,
        SubTitle = 0x00100,
        Task = 0x00200,
        DescriptionTop = 0x00400,
        DescriptionBottom = 0x00800,
        TitleImage = 0x01000,
        ExtendedImage = 0x02000,
        Items = 0x04000,
        Subset = 0x08000,
        SubsetItems = 0x10000
    }

    [Flags]
    public enum ListViewGroupState : uint
    {
        /// <summary>
        /// Groups are expanded, the group name is displayed, and all items in the group are displayed.
        /// </summary>
        Normal = 0,
        /// <summary>
        /// The group is collapsed.
        /// </summary>
        Collapsed = 1,
        /// <summary>
        /// The group is hidden.
        /// </summary>
        Hidden = 2,
        /// <summary>
        /// Version 6.00 and Windows Vista. The group does not display a header.
        /// </summary>
        NoHeader = 4,
        /// <summary>
        /// Version 6.00 and Windows Vista. The group can be collapsed.
        /// </summary>
        Collapsible = 8,
        /// <summary>
        /// Version 6.00 and Windows Vista. The group has keyboard focus.
        /// </summary>
        Focused = 16,
        /// <summary>
        /// Version 6.00 and Windows Vista. The group is selected.
        /// </summary>
        Selected = 32,
        /// <summary>
        /// Version 6.00 and Windows Vista. The group displays only a portion of its items.
        /// </summary>
        SubSeted = 64,
        /// <summary>
        /// Version 6.00 and Windows Vista. The subset link of the group has keyboard focus.
        /// </summary>
        SubSetLinkFocused = 128,
    }

}