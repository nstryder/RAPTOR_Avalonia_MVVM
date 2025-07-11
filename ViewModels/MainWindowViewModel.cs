using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.DTO;
using Avalonia.Controls;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using raptor;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Avalonia.Input;
using ReactiveUI;
using System.Reactive;
using interpreter;
using numbers;
using parse_tree;
using System.Timers;
using System.Threading;
using RAPTOR_Avalonia_MVVM.Views;
using RAPTOR_Avalonia_MVVM.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RAPTOR_Avalonia_MVVM.ViewModels;
using System.Collections;
using Avalonia.Threading;
using System.Diagnostics;
using System.Linq;

using Avalonia.Media;
using System.Security.Cryptography.X509Certificates;



namespace RAPTOR_Avalonia_MVVM.ViewModels
{



    public class MainWindowViewModel : ViewModelBase
    {
        private string My_Title = "Raptor";
        public bool AmLoading = false;
        public void Clear_Undo()
        {
            Undo_Stack.Clear_Undo(/*this*/);
        }
        
        public static bool ReverseLoopLogicToggle
        {
            get => Component.reverse_loop_logic;
            set => Component.reverse_loop_logic = value;
        }

        public int mainIndex
        {
            get
            {
                if (Component.Current_Mode == Mode.Expert)
                    return 1;
                else
                    return 0;
            }
        }
        public float scale = 1.0f;
        public Component? clipboard;
        public logging_info? log = new logging_info();
        public bool modified = false;
        public bool runningState = false;
        public string Text = "sdfasdf";
        private System.Guid file_guid_back = System.Guid.NewGuid();
        public Component? Current_Selection = null;
        public System.DateTime last_autosave = System.DateTime.Now;
        public System.Guid file_guid
        {
            get
            {
                return file_guid_back;
            }
            set
            {
                file_guid_back = value;
                /*if (Component.BARTPE)
                {
                    this.MC.Text = file_guid_back.ToString().Substring(0, 8) + ": Console";
                }*/
            }
        }
        public string? fileName;

        private ObservableCollection<Subchart>? privateTheTabs;
        public ObservableCollection<Subchart>? theTabs {
            get
            {
                return privateTheTabs;
            }
            set
            {
                privateTheTabs = value;
            }
        }

        private ObservableCollection<Variable> privateTheVariables;
        public ObservableCollection<Variable> theVariables
        {
            get
            {
                return privateTheVariables;
            }
            set
            {
                privateTheVariables = value;
            }
        }
        public void Update_View_Variables()
        {

        }
        public Subchart mainSubchart()
        {
            return this.theTabs[0];
        }

        public static List<Subchart> FillTabs() {
            Subchart main_subchart = new Subchart("main");
            return new List<Subchart>(){
                main_subchart
            };
        }
        public static List<Variable> FillWatch()
        {
            //Fill league data here...
            return new List<Variable>()
            {
            };

        }
        public int x = 0;
        /*public ObservableCollection<MenuItem> ContextMenuItemsFunction()
        {
            return new ObservableCollection<MenuItem> { new MenuItem() { Header = "Hello" }, new MenuItem() { Header = "World" } };
        }*/

        public bool shouldClose = true;

        public async Task setShouldClose() {

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                string msg = "Do you want to save your changes first?";
                await MessageBoxClass.Show(msg, "Save First?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (buttonAnswer == ButtonResult.Yes)
                {
                    shouldClose = false;
                    OnSaveCommand(true);
                }
                else if (buttonAnswer == ButtonResult.No)
                {
                    shouldClose = false;
                    MainWindow.topWindow.Close();
                }
                else
                {
                    shouldClose = true;
                }

            });


        }


        public async void OnClosingCommand()
        {
            if (modified)
            {

                await Dispatcher.UIThread.InvokeAsync(async () => { await setShouldClose(); });

            }
            else
            {
                shouldClose = false;
                MainWindow.topWindow.Close();
            }
        }
        public static MainWindowViewModel theMainWindowViewModel;
        public static MainWindowViewModel GetMainWindowViewModel()
        {
            return theMainWindowViewModel;
        }

        public MainWindowViewModel()
        {
            theMainWindowViewModel = this;
            theVariables = new ObservableCollection<Variable>(FillWatch());
            //Subchart main_subchart = new Subchart("main");
            theTabs = new ObservableCollection<Subchart>(FillTabs());
            Plugins.Load_Plugins("");
            raptor.Generators.Load_Generators();


            string[] args = App.getArgs();
            if (args != null && args.Length>=1 && args[0] != "")
            {
                Load_FileAsync(args[0]);
            }


        }
        public string Greeting => "Welcome to Avalonia!";
        private Window GetWindow()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                return desktopLifetime.MainWindow;
            }
            return null;
        }
        public void Clear_Subcharts()
        {
            this.theTabs.Clear();
            this.theTabs.Add(new Subchart());
        }
        public async Task Load_FileAsync(string dialog_fileName)
        {

            Stream stream;
            FileAttributes attr;
            try
            {
                stream = File.Open(dialog_fileName, FileMode.Open,
                        FileAccess.Read);
                attr = System.IO.File.GetAttributes(dialog_fileName);
                MainWindow.setMainTitle("RAPTOR - " + dialog_fileName);
            }
            catch
            {
                MessageBoxClass.Show("Unable to open file: " + dialog_fileName);
                return;
            }

            try
            {
                AmLoading = true;
                if (dialog_fileName.EndsWith(".oldrap"))
                {
                    await Load_Old_Raptor_File(dialog_fileName, stream);
                }
                else
                {
                    await Load_New_Raptor_File(dialog_fileName, stream);
                }
                Post_Load_Method(dialog_fileName, attr);
            }
            catch (System.Exception e)
            {
                MessageBoxClass.Show(e.Message + "\n" + e.StackTrace + "\n" + "Invalid Filename:" + dialog_fileName, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            AmLoading = false;
        }
        [KnownType(typeof(logging_info.event_kind))]
        [KnownType(typeof(Component.FootPrint))]
        [KnownType(typeof(Oval))]
        [KnownType(typeof(Oval_Procedure))]
        [KnownType(typeof(Oval_Return))]
        [KnownType(typeof(Parallelogram))]
        [KnownType (typeof(IF_Control))]
        [KnownType(typeof(Loop))]
        [KnownType(typeof(Rectangle))]
        [KnownType(typeof(Loop))]
        [KnownType(typeof(Rectangle.Kind_Of))]
        [KnownType(typeof(raptor.CommentBox))]
        [KnownType(typeof(Procedure_Chart))]
        [KnownType(typeof(Subchart))]
        [DataContract]
        private class Raptor_File
        {
            [DataMember]
            public System.Guid FileGuid { get; set; }
            [DataMember]
            public logging_info? log;
            [DataMember]
            public Subchart[]? subcharts;
            public Raptor_File()
            {

            }
            public Raptor_File(Guid fileGuid, logging_info? log, int num_charts, ObservableCollection<Subchart> subcharts)
            {
                FileGuid = fileGuid;
                this.log = log;
                this.subcharts = new Subchart[num_charts];
                for (int i = 0; i < num_charts; i++)
                {
                    this.subcharts[i] = subcharts[i];
                }
            }
        }
        private async Task Load_New_Raptor_File(string dialog_fileName, Stream stream)
        {
            try
            {
                Component.warned_about_newer_version = false;
                Component.warned_about_error = false;
                //this.Clear_Subcharts();
                this.theTabs.Clear(); // get rid of all, including main
                DataContractSerializer dcs = new DataContractSerializer(typeof(Raptor_File));
                Raptor_File rf = (Raptor_File) dcs.ReadObject(stream);
                foreach (Subchart sc in rf.subcharts) {
                    this.theTabs.Add(sc);
                }
                this.log = rf.log;
                this.file_guid = rf.FileGuid;
                stream.Close();
            }
            catch (System.Exception e)
            {
                MessageBoxClass.Show("Invalid File-not a flowchart, aborting"+e.Message,
                    "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.OnNewCommand();
                try
                {
                    stream.Close();
                }
                catch
                {
                }
                return;
            }
        }

        private async Task Load_Old_Raptor_File(string dialog_fileName, Stream stream)
        {
            try
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                Component.warned_about_newer_version = false;
                Component.warned_about_error = false;
                this.Clear_Subcharts();
                try
                {
                    // starting with version 11, we put the version number first
                    // WARNING!  If you change this, you'll have to change
                    // this similar code in MasterConsole that does 
                    // extract_times
                    Component.last_incoming_serialization_version = (int)bformatter.Deserialize(stream);
                    bool incoming_reverse_loop_logic;
                    if (Component.last_incoming_serialization_version >= 13)
                    {
                        incoming_reverse_loop_logic = (bool)bformatter.Deserialize(stream);
                    }
                    else
                    {
                        incoming_reverse_loop_logic = false;
                    }

                    // read in number of pages
                    int num_pages = (int)bformatter.Deserialize(stream);
                    for (int i = 0; i < num_pages; i++)
                    {
                        Subchart_Kinds incoming_kind;
                        string name = (string)bformatter.Deserialize(stream);
                        if (Component.last_incoming_serialization_version >= 14)
                        {
                            object o = bformatter.Deserialize(stream);
                            incoming_kind = (Subchart_Kinds)o;
                        }
                        else
                        {
                            incoming_kind = Subchart_Kinds.Subchart;
                        }
                        if (i == 0 && incoming_kind != Subchart_Kinds.UML &&
                            Component.Current_Mode == Mode.Expert)
                        {
                            MessageBoxClass.Show("Changing to Intermediate Mode");
                            //this.menuIntermediate_Click(null, null);
                        }
                        if (incoming_kind != Subchart_Kinds.Subchart)
                        {
                            if (Component.Current_Mode != Mode.Expert &&
                                incoming_kind == Subchart_Kinds.UML)
                            {
                                MessageBoxClass.Show("Can't open OO RAPTOR file");
                                //MessageBox.Show("Changing to Object-Oriented Mode");
                                //this.menuObjectiveMode_Click(null, null);
                                throw new Exception("unimplemented");
                            }
                            if (Component.Current_Mode == Mode.Novice)
                            {
                                MessageBoxClass.Show("Changing to Intermediate Mode");
                                //this.menuIntermediate_Click(null, null);
                            }
                        }
                        // I moved these down lower in case the mode was changed by
                        // reading in this flowchart (which calls new and clears filename)
                        this.fileName = dialog_fileName;
                        Plugins.Load_Plugins(this.fileName);

                        if (i > mainIndex)
                        {
                            int param_count = 0;
                            switch (incoming_kind)
                            {
                                case Subchart_Kinds.Function:
                                    param_count = (int)bformatter.Deserialize(stream);
                                    //this.theTabs.Add()
                                    this.theTabs.Add(new Procedure_Chart(name,
                                        param_count));
                                    break;
                                case Subchart_Kinds.Procedure:
                                    if (Component.last_incoming_serialization_version >= 15)
                                    {
                                        param_count = (int)bformatter.Deserialize(stream);
                                    }
                                    this.theTabs.Add(new Procedure_Chart(name,
                                        param_count));
                                    break;
                                case Subchart_Kinds.Subchart:
                                    this.theTabs.Add(new Subchart(name));
                                    break;
                            }
                        }
                    }

                    Component.negate_loops = false;
                    /*if (Component.Current_Mode == Mode.Expert)
                    {
                        NClass.Core.BinarySerializationHelper.diagram =
                            (this.theTabs[0].Controls[0] as UMLDiagram).diagram;
                        (this.theTabs[0].Controls[0] as UMLDiagram).project.LoadBinary(
                            bformatter, stream);
                    }
                    else */
                    // if (incoming_reverse_loop_logic != Component.reverse_loop_logic)
                    // {
                    //     Component.negate_loops = true;
                    // }
                    for (int i = mainIndex; i < num_pages; i++)
                    {
                        object o = bformatter.Deserialize(stream);
                        ((Subchart)this.theTabs[i]).Start = (Oval)o;
                        ((Subchart)this.theTabs[i]).Start.scale = this.scale;
                        ((Subchart)this.theTabs[i]).Start.Scale(this.scale);
                        if (Component.last_incoming_serialization_version >= 17)
                        {
                            byte[] ink = (byte[])bformatter.Deserialize(stream);
                            /*if (!Component.BARTPE && !Component.MONO && ink.Length > 1)
                            {
                                bool was_enabled = ((Subchart)this.theTabs[i]).tab_overlay.Enabled;
                                ((Subchart)this.theTabs[i]).tab_overlay.Enabled = false;
                                ((Subchart)this.theTabs[i]).tab_overlay.Ink = new Microsoft.Ink.Ink();
                                ((Subchart)this.theTabs[i]).tab_overlay.Ink.Load(ink);
                                ((Subchart)this.theTabs[i]).tab_overlay.Enabled = was_enabled;
                                ((Subchart)this.theTabs[i]).scale_ink(this.scale);
                            }
                            else if (((Subchart)this.theTabs[i]).tab_overlay != null)
                            {
                                bool was_enabled = ((Subchart)this.theTabs[i]).tab_overlay.Enabled;
                                ((Subchart)this.theTabs[i]).tab_overlay.Enabled = false;
                                ((Subchart)this.theTabs[i]).tab_overlay.Ink = new Microsoft.Ink.Ink();
                                ((Subchart)this.theTabs[i]).tab_overlay.Enabled = was_enabled;
                                ((Subchart)this.theTabs[i]).scale_ink(this.scale);
                            }*/
                        }
                        this.Current_Selection = ((Subchart)this.theTabs[i]).Start.select(-1000, -1000, FlowchartControl.ctrl);
                    }
                    //this.carlisle.SelectedTab = this.mainSubchart();
                }
                catch (System.Exception e)
                {
                    // previous to version 11, there is just one tab page
                    // moved this way down here for very old files (previous to version 11)
                    this.fileName = dialog_fileName;
                    Plugins.Load_Plugins(this.fileName);
                    stream.Seek(0, SeekOrigin.Begin);
                    Runtime.consoleWriteln(e.Message);
                    this.mainSubchart().Start = (Oval)bformatter.Deserialize(stream);
                    Component.last_incoming_serialization_version =
                       this.mainSubchart().Start.incoming_serialization_version;
                }

                // load all of the subcharts based on what the UML Diagram created for tabs
                /*if (Component.Current_Mode == Mode.Expert)
                {
                    for (int i = mainIndex + 1; i < this.theTabs.Count; i++)
                    {
                        ClassTabPage ctp = this.theTabs[i] as ClassTabPage;
                        for (int j = 0; j < ctp.tabControl1.TabPages.Count; j++)
                        {
                            Subchart sc = ctp.tabControl1.TabPages[j] as Subchart;
                            sc.Start = (Oval)bformatter.Deserialize(stream);
                            sc.Start.scale = this.scale;
                            sc.Start.Scale(this.scale);
                            byte[] ink = (byte[])bformatter.Deserialize(stream);
                            if (!Component.BARTPE && ink.Length > 1)
                            {
                                bool was_enabled = sc.tab_overlay.Enabled;
                                sc.tab_overlay.Enabled = false;
                                sc.tab_overlay.Ink = new Microsoft.Ink.Ink();
                                sc.tab_overlay.Ink.Load(ink);
                                sc.tab_overlay.Enabled = was_enabled;
                                sc.scale_ink(this.scale);
                            }
                            else if (sc.tab_overlay != null)
                            {
                                bool was_enabled = sc.tab_overlay.Enabled;
                                sc.tab_overlay.Enabled = false;
                                sc.tab_overlay.Ink = new Microsoft.Ink.Ink();
                                sc.tab_overlay.Enabled = was_enabled;
                                sc.scale_ink(this.scale);
                            }
                            this.Current_Selection = sc.Start.select(-1000, -1000);
                        }
                    }
                }*/
                if (Component.last_incoming_serialization_version >= 4)
                {
                    this.log = (logging_info)bformatter.Deserialize(stream);
                }
                else
                {
                    this.log.Clear();
                }
                if (Component.last_incoming_serialization_version >= 6)
                {
                    Component.compiled_flowchart = (bool)bformatter.Deserialize(stream);
                }
                else
                {
                    Component.compiled_flowchart = false;
                }
                if (Component.last_incoming_serialization_version >= 8)
                {
                    this.file_guid = (System.Guid)bformatter.Deserialize(stream);
                }
                else
                {
                    this.file_guid = System.Guid.NewGuid();
                }
                if (Component.compiled_flowchart)
                {
                    MessageBoxClass.Show("Compiled flowchart not supported");
                    //MessageBox.Show("Changing to Object-Oriented Mode");
                    //this.menuObjectiveMode_Click(null, null);
                    throw new Exception("unimplemented");
                    /*Registry_Settings.Ignore_Updates = true;
                    this.trackBar1.Value = this.trackBar1.Maximum;
                    this.trackBar1_Scroll(null, null);
                    if (this.menuViewVariables.Checked)
                    {
                        this.menuViewVariables_Click(null, null);
                    }
                    Registry_Settings.Ignore_Updates = false;

                    Compile_Helpers.Compile_Flowchart(this.theTabs);*/
                }
                if (Component.Current_Mode != Mode.Expert)
                {
                    /*for (int i = mainIndex; i < this.carlisle.TabCount; i++)
                    {
                        ((Subchart)this.theTabs[i]).flow_panel.Invalidate();
                    }*/
                }
                else
                {/*
                        ((Subchart)this.theTabs[mainIndex]).flow_panel.Invalidate();
                        for (int i = mainIndex + 1; i < this.carlisle.TabCount; i++)
                        {
                            for (int j = 0; j < (this.theTabs[i] as ClassTabPage).tabControl1.TabCount; j++)
                            {
                                Subchart sc = (this.theTabs[i] as ClassTabPage).tabControl1.TabPages[j] as Subchart;
                                sc.flow_panel.Invalidate();
                            }
                        }*/
                }
                this.log.Record_Open();
                stream.Close();
            }
            catch
            {
                /*if (command_line_run)
                {
                    stream.Close();
                    return;
                }*/
                MessageBoxClass.Show("Invalid File-not a flowchart, aborting",
                    "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.OnNewCommand();
                try
                {
                    stream.Close();
                }
                catch
                {
                }
                return;
            }
        }

        private void Post_Load_Method(string dialog_fileName, FileAttributes attr)
        {
            this.Update_View_Variables();

            Environment.CurrentDirectory = Path.GetDirectoryName(dialog_fileName);
            Runtime.Clear_Variables();

            this.runningState = false;
            //MRU.Add_To_MRU_Registry(this.fileName);
            this.Text = My_Title + " - " +
                Path.GetFileName(this.fileName);
            if ((attr & FileAttributes.ReadOnly) > 0)
            {
                this.Text = this.Text + " [Read-Only]";
            }
            this.modified = false;


            this.mainSubchart().Start.scale = this.scale;
            this.mainSubchart().Start.Scale(this.scale);
            this.Current_Selection = this.mainSubchart().Start.select(-1000, -1000, FlowchartControl.ctrl);
            this.Clear_Undo();


            /*if (this.menuAllText.Checked)
            {
                this.menuAllText_Click(null, null);
            }
            else if (this.menuTruncated.Checked)
            {
                this.menuTruncated_Click(null, null);
            }
            else
            {
                this.menuNoText_Click(null, null);
            }*/
            //Component.view_comments = this.menuViewComments.Checked;
            // can only Invalidate the flow_panel if it has had its handle created (i.e. not in /run mode)
            /*if (flow_panel.IsHandleCreated)
            {
                flow_panel.Invalidate();
            }*/
            RAPTOR_Avalonia_MVVM.ViewModels.MasterConsoleViewModel.MC.clear_txt();
        }

        public async void OnOpenCommand()
        {
            if (modified)
            {
                await OnNewCommand();
            }
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filters.Add(new FileDialogFilter() { Name = "RAPTOR", Extensions = { "rap" } });
            dialog.Filters.Add(new FileDialogFilter() { Name = "Old RAPTOR", Extensions = { "oldrap" } });
            dialog.Filters.Add(new FileDialogFilter() { Name = "All Files", Extensions = { "*" } });
            dialog.AllowMultiple = false;
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                string[] result = await dialog.ShowAsync(desktop.MainWindow);
                if (result != null)
                {
                    if (result.Length > 0)
                    {
                        
                        Load_FileAsync(result[0]);
                        this.OnResetCommand();
                    }
                    /*var msBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                        {
                            ButtonDefinitions = ButtonEnum.Ok,
                            ContentTitle = "Opening",
                            ContentMessage = result[0],
                            Icon = Icon.Plus,
                            Style = Style.Windows
                        });

                    ButtonResult br= await msBoxStandardWindow.ShowDialog(desktop.MainWindow);*/
                }
            }

        }
        public void OnEditCommand()
        {
            this.theTabs[this.activeTab].Start.setText(this.theTabs[this.activeTab].positionX, this.theTabs[this.activeTab].positionY);
        }
        public void OnCutCommand()
        {
            Component cutReturn = this.theTabs[this.viewTab].Start.cut();
            if (cutReturn != null)
            {
                Clipboard_Data cd = new Clipboard_Data(
                cutReturn,
                this.file_guid, this.log.Clone());
                ClipboardMultiplatform.SetDataObject(cd, true);
            }
        }
        public void OnExitCommand()
        {
            GetWindow().Close();
        }
        public void OnDeleteCommand()
        {
            this.theTabs[this.viewTab].Start.delete();

        }

        public async void OnPasteCommand() {
            int x_position = this.theTabs[this.viewTab].positionXTapped;
            int y_position = this.theTabs[this.viewTab].positionYTapped;

            try
            {
                Object thing = await ClipboardMultiplatform.GetDataObjectAsync();
                if (thing != null)
                {
                    Component obj = ((Clipboard_Data)thing).symbols;
                    Undo_Stack.Make_Undoable(this.theTabs[this.viewTab]);
                    Component the_clone = obj.Clone();
                    this.theTabs[this.viewTab].Start.insert(the_clone, x_position, y_position, 0);
                }
            }
            catch
            {

            }


        }
        public void OnCopyCommand() {
            Component copyReturn = this.theTabs[this.viewTab].Start.copy();
            if (copyReturn != null)
            {
                Clipboard_Data cd = new Clipboard_Data(
                copyReturn,
                this.file_guid, this.log.Clone());
                ClipboardMultiplatform.SetDataObject(cd, true);
            }
        }

        public string Get_Autosave_Name()
        {
            char c;
            System.DateTime oldest_time = System.DateTime.MaxValue;
            System.DateTime this_time;
            string oldest = this.fileName + ".backup0";

            try
            {
                for (c = '0'; c <= '3'; c++)
                {
                    string this_name = this.fileName + ".backup" + c;

                    if (System.IO.File.Exists(this_name))
                    {
                        this_time = System.IO.File.GetLastWriteTime(this_name);
                        if (this_time < oldest_time)
                        {
                            oldest_time = this_time;
                            oldest = this_name;
                        }
                    }
                    else
                    {
                        return this_name;
                    }
                }
            }
            catch
            {
            }
            return oldest;
        }

        public void Perform_Autosave()
        {
            string name = this.Get_Autosave_Name();
            this.Perform_Save(name, true);
        }

        public async Task OnSaveCommand(bool closeAfter = false) {

            await FileSave_Click(closeAfter);

        }

        public void OnSaveCommand2()
        {

            FileSave_Click(false);

        }

        public async Task FileSave_Click(bool closeAfter = false)
        {
            if (fileName == "" || fileName == null)
            {
                await this.OnSaveAsCommand(closeAfter);
            }
            else
            {
                await this.Perform_Save(this.fileName, false, closeAfter);
                Plugins.Load_Plugins(this.fileName);
            }
        }

        private bool Save_Error = false;
        private async Task Perform_Save(string name, bool is_autosave, bool closeAfter = false)
        {
            Stream stream;
            string prefix;
            this.last_autosave = System.DateTime.Now;

            if (is_autosave)
            {
                prefix = "Error during autosave:";
            }
            else
            {
                prefix = "Error during save:";
            }

            try
            {
                stream = File.Open(name, FileMode.Create);
            }
            catch
            {
                if (File.Exists(name) &&
                    (File.GetAttributes(name) & FileAttributes.ReadOnly) > 0)
                {
                    await MessageBoxClass.Show(
                        prefix + '\n' +
                        name + " is a read-only file",
                        "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    await MessageBoxClass.Show(
                        prefix + '\n' +
                        "Unable to create file: " +
                        name, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                this.Save_Error = true;
                return;
            }

            if (name.EndsWith(".oldrap"))
            { 
                try
                {

                    BinaryFormatter bformatter = new BinaryFormatter();
                    //bformatter.Serialize(stream, 
                    bformatter.Serialize(stream, Component.current_serialization_version);
                    // USMA_mode new in file version 13
                    bformatter.Serialize(stream, Component.reverse_loop_logic);

                    bformatter.Serialize(stream, theTabs.Count);

                    for (int i = mainIndex; i < theTabs.Count; i++)
                    {
                        bformatter.Serialize(stream, this.theTabs[i].Text);
                        // subchart kind is new in file version 14
                        bformatter.Serialize(stream, ((Subchart)this.theTabs[i]).Subchart_Kind);
                        if (((Subchart)this.theTabs[i]) is Procedure_Chart)
                        {
                            bformatter.Serialize(stream, ((Procedure_Chart)this.theTabs[i]).num_params);
                        }
                    }

                    for (int i = mainIndex; i < this.theTabs.Count; i++)
                    {
                        bformatter.Serialize(stream, ((Subchart)this.theTabs[i]).Start);
                        // new in version 17
                        byte[] output;
                        if (!Component.BARTPE && !Component.VM && !Component.MONO)
                        {
                            //output = ((Subchart)this.theTabs[i]).tab_overlay.Ink.Save();
                            output = new byte[1];
                        }
                        else
                        {
                            output = new byte[1];
                        }
                        bformatter.Serialize(stream, output);
                    }

                    if (!is_autosave)
                    {
                        this.log.Record_Save();
                    }
                    else
                    {
                        this.log.Record_Autosave();
                    }

                    bformatter.Serialize(stream, this.log);
                    bformatter.Serialize(stream, Component.compiled_flowchart);
                    bformatter.Serialize(stream, this.file_guid);
                    stream.Close();
                    this.Save_Error = false;
                    if (!is_autosave)
                    {
                        this.modified = false;
                    }


                }
                catch (System.Exception exc)
                {
                    MessageBoxClass.Show(
                        prefix + '\n' +
                        "Please report to carlislem@tamu.edu" + '\n' +
                        "Meantime, try undo then save (keep doing undo until success)" + '\n' +
                        "Or open an autosave file: " + this.fileName + ".[0-9]" + '\n' +
                        "Use Alt-PrtSc and paste into email" + '\n' +
                        exc.Message + '\n' +
                        exc.StackTrace, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Save_Error = true;
                }
            }
            else
            {
                try
                {
                    Raptor_File rf = new Raptor_File(this.file_guid, this.log, this.theTabs.Count, this.theTabs);
                    DataContractSerializer dcs= new DataContractSerializer(typeof(Raptor_File));
                    dcs.WriteObject(stream, rf);
                    stream.Close();

                }
                catch (System.Exception exc)
                {
                    MessageBoxClass.Show(
                        prefix + '\n' +
                        "Please report to carlislem@tamu.edu" + '\n' +
                        "Meantime, try undo then save (keep doing undo until success)" + '\n' +
                        "Or open an autosave file: " + this.fileName + ".[0-9]" + '\n' +
                        "Use Alt-PrtSc and paste into email" + '\n' +
                        exc.Message + '\n' +
                        exc.StackTrace, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Save_Error = true;
                }
            }

            if (closeAfter)
            {
                MainWindow.topWindow.Close();
            }

        }

        public async void OnSaveAsCommand2()
        {
            OnSaveAsCommand(false);
        }

        public async Task OnSaveAsCommand(bool closeAfter = false) {

            string dialog_fileName;
            string oldName = this.fileName;

            if (!Component.BARTPE)
            {
                SaveFileDialog fileChooser = new SaveFileDialog();

                List<FileDialogFilter> Filters = new List<FileDialogFilter>();
                FileDialogFilter filter = new FileDialogFilter();
                List<string> extension = new List<string>();
                extension.Add("rap");
                filter.Extensions = extension;
                filter.Name = "Raptor Files";
                Filters.Add(filter);
                FileDialogFilter filter2 = new FileDialogFilter();
                List<string> extension2 = new List<string>();
                extension2.Add("oldrap");
                filter2.Extensions = extension2;
                filter2.Name = "Old Raptor Files";
                Filters.Add(filter2);
                fileChooser.Filters = Filters;

                fileChooser.DefaultExtension = "rap";

                string ans = await fileChooser.ShowAsync(MainWindow.topWindow);

                if (ans == null || ans == "")
                {
                    return;
                }

                this.fileName = ans;
                MainWindow.setMainTitle("RAPTOR - " + ans);
                Plugins.Load_Plugins(this.fileName);
                await this.FileSave_Click(closeAfter);

            }

        }

        public Component currentActiveComponent;
        public Component activeComponent {
            get { return currentActiveComponent; }
            set { currentActiveComponent = value; }
        }

        public Component currentParentComponent;
        public Component parentComponent {
            get { return currentParentComponent; }
            set { currentParentComponent = value; }
        }

        public int currentInLoop = 0;
        public int inLoop {
            get { return currentInLoop; }
            set { currentInLoop = value; }
        }

        public int currentInSelection = 0;
        public int inSelection {
            get { return currentInSelection; }
            set { currentInSelection = value; }
        }

        public int symbolCount = 0;
        public int decreaseScope = 0;
        public int decreaseSub = 0;
        public bool isSub = false;

        public ObservableCollection<Component> parentCount = new ObservableCollection<Component>();

        public ObservableCollection<string> activeScopes = new ObservableCollection<string>() { "main" };

        private bool getParent = false;
        public bool waitingForKey = false;
        public bool waitingForMouse = false;
        public Avalonia.Input.MouseButton mouseWait;
        public void aboutToRunComponent(Component c)
        {
            if (c.break_now())
            {
                if (myTimer != null)
                {
                    OnPauseCommand();
                }
            }
            if (c.selected)
            {
                c.selected = false;
            }
            ScrollViewer sv = (FlowchartControl.fcc.Parent as UserControl).Parent as ScrollViewer;
            Dispatcher.UIThread.InvokeAsync(async() => sv.Offset = new Avalonia.Vector(c.X, c.Y));
            c.running = true;
        }
        public void goToNextComponent() { 

            if (waitingForKey || waitingForMouse)
            {
                return;
            }
            try
            {
                symbolCount++;
                if (parentCount.Count != 0 && parentComponent == null)
                {
                    parentComponent = parentCount[parentCount.Count - 1];
                    parentCount.RemoveAt(parentCount.Count - 1);
                }
                activeComponent.running = false;
                if (activeComponent.GetType() != typeof(Oval) || activeComponent.Successor != null)
                {
                    Component temp = Find_Successor(activeComponent);
                    activeComponent = temp;
                }
                else
                {
                    // We are in an Oval with no successor (end of a routine)
                    if (isSub)
                    {
                        decreaseSub--;
                        if (activeTabs.Count > 1)
                        {
                            activeTab = activeTabs[activeTabs.Count - 2];
                            activeTabs.RemoveAt(activeTabs.Count - 1);
                        }
                        else
                        {
                            activeTab = 0;
                        }
                        isSub = false;
                    }
                    else if (decreaseScope != 0)
                    {
                        Runtime.Decrease_Scope();
                        decreaseScope--;
                        if (activeTabs.Count > 1)
                        {
                            activeTab = activeTabs[activeTabs.Count - 2];
                            activeTabs.RemoveAt(activeTabs.Count - 1);
                        }
                        else
                        {
                            activeTab = 0;
                        }
                        activeScopes.RemoveAt(activeScopes.Count - 1);
                    }
                    parentComponent.running = false;
                    activeComponent.running = false;
                    if (parentCount.Count != 0)
                    {
                        parentComponent = parentCount[parentCount.Count - 1];
                        parentCount.RemoveAt(parentCount.Count - 1);
                    }
                    parentComponent.running = false;
                    if (parentComponent.Successor != null)
                    {
                        activeComponent = parentComponent.Successor.First_Of();
                    }
                    else
                    {
                        activeComponent = parentComponent;
                        goToNextComponent();
                    }
                }
                aboutToRunComponent(activeComponent);
            }
            catch (Exception e)
            {
                if(myTimer != null)
                {
                    myTimer.Stop();
                }
                Runtime.consoleWriteln("--- Run Halted! ---\n" + e.Message);
            }
        }

        private bool varFound(string s) {
            return Runtime.getAnyVariable(s, activeScopes[activeScopes.Count - 1]);
        }

        public static Component Find_Successor(Component c)
        {
            //if (c.GetType()==typeof(Loop) && ((Loop) c).light_head)
            //{

            //}
            // if I've got a successor, just go there!
            if (c.Successor != null)
            {
                //return c.Successor; // mcc: I think I might should say First_Of, but that seems to light the head in confusing ways.
                return c.Successor.First_Of();
            }
            if (c.parent == null)
            {
                throw new System.Exception(
                    "I have no successor or parent!");
            }
            // if I'm the child of an IF statement, then I just
            // want the successor of my parent
            if (c.parent.GetType() == typeof(IF_Control))
            {
                return Find_Successor(c.parent);
            }
            else if (c.parent.GetType() == typeof(Loop))
            {
                // if I'm the before child of a loop,
                // then go to the loop (test)
                if (c.is_beforeChild)
                {
                    return c.parent;
                }
                else
                {
                    // since I'm an after child, I just want
                    // to go back to the top of the loop
                    return c.parent.First_Of();
                }
            }
            else
            {
                throw new Exception(
                    "My parent isn't a loop or if_control!");
            }
        }


        public async void OnNextCommand() {
            try
            {
                if (activeComponent == null)
                {
                    startRun();
                    activeComponent = this.mainSubchart().Start;
                    activeComponent.selected = false;
                    activeComponent.running = true;
                }
                else
                {
                    if ((activeComponent.GetType() == typeof(Oval) && activeComponent.Successor == null && activeTab == 0 && inLoop == 0 && inSelection == 0) || (activeComponent.GetType() == typeof(Oval) && activeComponent.Successor == null && parentComponent == null && activeTab == 0))
                    {
                        symbolCount++;
                        raptor_files.Stop_Redirect_Output();
                        raptor_files.Stop_Redirect_Input();
                        Runtime.consoleWrite("--- Run Complete! " + symbolCount + " Symbols Evaluated ---\n");


                        activeComponent.running = false;
                        activeComponent = null;
                        if (myTimer != null)
                        {
                            myTimer.Stop();
                            myTimer = null;
                        }
                        parentCount.Clear();
                        parentComponent = null;
                        decreaseScope = 0;
                        activeTab = 0;
                        return;
                    }
                    else if (activeComponent.GetType() == typeof(Oval) && activeComponent.Successor == null && parentComponent != null)
                    {

                        Subchart activeSubchart = theTabs[activeTab];
                        symbolCount++;
                        activeComponent.running = false;

                        if (activeSubchart.Start.GetType() != typeof(Oval_Procedure))
                        {
                            isSub = true;
                            goToNextComponent();
                            setViewTab = activeTab;
                            return;
                        }

                        Oval_Procedure tempStart = (Oval_Procedure)activeSubchart.Start;
                        ObservableCollection<Object> outVals = new ObservableCollection<Object>();

                        for (int i = 0; i < tempStart.param_names.Length; i++)
                        {
                            if (tempStart.param_is_output[i])
                            {
                                Variable tempVar = Runtime.Lookup_Variable(tempStart.param_names[i]);
                                if (tempVar.Kind == Runtime.Variable_Kind.Value)
                                {
                                    numbers.value outVal = Runtime.getVariable(tempStart.param_names[i]);
                                    outVals.Add(outVal);
                                }
                                else if (tempVar.Kind == Runtime.Variable_Kind.One_D_Array)
                                {
                                    numbers.value[] outVal = Runtime.getValueArray(tempStart.param_names[i]);
                                    outVals.Add(outVal);
                                }
                                else if (tempVar.Kind == Runtime.Variable_Kind.Two_D_Array)
                                {
                                    numbers.value[][] outVal = Runtime.get2DValueArray(tempStart.param_names[i]);
                                    outVals.Add(outVal);
                                }

                            }
                        }

                        //string[] textStr = parentComponent.text_str.Split("(")[1].Split(","); // wont work for array[3,5] 

                        //ObservableCollection<string> textStr = getParamNames(parentComponent.text_str);
                        goToNextComponent();
                        setViewTab = activeTab;

                        string[] textStr = parentComponent.text_str.Split("(")[1].Split(",");

                        int spot = 0;
                        for (int i = 0; i < outVals.Count; i++)
                        {
                            for (int k = spot; k < tempStart.param_names.Length; k++)
                            {
                                if (tempStart.param_is_output[k])
                                {
                                    textStr[k] = textStr[k].Replace(")", "").Replace(" ", "");
                                    Variable tempVar = Runtime.Lookup_Variable(textStr[k]);
                                    if (tempVar == null)
                                    {
                                        throw new Exception("Variable " + textStr[k] + " not found!");
                                    }
                                    if (tempVar.Kind == Runtime.Variable_Kind.Value)
                                    {
                                        Runtime.setVariable(textStr[k], (numbers.value)outVals[i]);
                                        spot = k + 1;
                                        break;
                                    }
                                    else if (tempVar.Kind == Runtime.Variable_Kind.One_D_Array)
                                    {
                                        numbers.value[] arr = (numbers.value[])outVals[i];
                                        for (int n = 0; n < arr.Length; n++)
                                        {
                                            Runtime.setArrayElement(textStr[k], n + 1, arr[n]);
                                            spot = k + 1;
                                        }
                                        break;
                                    }
                                    else if (tempVar.Kind == Runtime.Variable_Kind.Two_D_Array)
                                    {
                                        numbers.value[][] arr = (numbers.value[][])outVals[i];
                                        for (int r = 0; r < arr.Length; r++)
                                        {
                                            for (int c = 0; c < arr[r].Length; c++)
                                            {
                                                Runtime.set2DArrayElement(textStr[k], r + 1, c + 1, arr[r][c]);
                                                spot = k + 1;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        return;
                    }
                    else if (activeComponent.GetType() == typeof(Oval))
                    {
                        goToNextComponent();
                        return;
                    }
                    else if (activeComponent.GetType() == typeof(Rectangle))
                    {
                        Rectangle temp = (Rectangle)activeComponent;
                        if (temp.kind == Rectangle.Kind_Of.Assignment)
                        {
                            string str = temp.text_str;
                            if (temp.text_str == "")
                            {
                                throw new Exception("Assignment not instantiated");
                            }
                            Lexer l = new Lexer(str);
                            if (temp.parse_tree != null)
                            {
                                Expr_Assignment ea = (Expr_Assignment)temp.parse_tree;
                                ea.Execute(l);
                                if (decreaseScope != 0 && !varFound(l.Get_Text(0, str.IndexOf(":"))))
                                {
                                    //decreaseScope--;
                                    Variable tempVar = theVariables[theVariables.Count - 1];
                                    theVariables.RemoveAt(theVariables.Count - 1);
                                    theVariables.Insert(1, tempVar);
                                }
                            }
                        }
                        else
                        {
                            string str = temp.text_str;
                            if (temp.text_str == "")
                            {
                                throw new Exception("Call not instantiated");
                            }
                            Lexer l = new Lexer(str);
                            if (temp.parse_tree != null)
                            {
                                Procedure_Call ea = (Procedure_Call)temp.parse_tree;
                                ea.Execute(l);
                            }
                        }
                        goToNextComponent();
                    }
                    else if (activeComponent.GetType() == typeof(IF_Control))
                    {
                        IF_Control temp = (IF_Control)activeComponent;
                        string str = temp.text_str;
                        if (temp.text_str == "")
                        {
                            throw new Exception("Selection not instantiated");
                        }
                        Lexer l = new Lexer(str);
                        if (temp.parse_tree != null)
                        {
                            Boolean_Expression r = (Boolean_Expression)temp.parse_tree;
                            bool rel = r.Execute(l); // evaluate the boolean expression
                            parentComponent = temp;
                            if (rel) // Go into the Yes branch as it evaluated to true
                            {
                                if (temp.left_Child != null) // we have a left child
                                {
                                    activeComponent.running = false;
                                    activeComponent = temp.left_Child.First_Of();
                                }
                                else // we don't have a left child
                                {
                                    activeComponent.running = false;
                                    activeComponent = Find_Successor(temp);
                                }
                                aboutToRunComponent(activeComponent);
                            }
                            else // Go into No branch as it evaluated to false
                            {
                                if (temp.right_Child != null) // we have a right child
                                {
                                    activeComponent.running = false;
                                    activeComponent = temp.right_Child.First_Of();
                                }
                                else // we don't have a right child
                                {
                                    activeComponent.running = false;
                                    activeComponent = Find_Successor(temp);
                                }
                                aboutToRunComponent(activeComponent);
                            }
                        }
                    }
                    else if (activeComponent.GetType() == typeof(Loop))
                    {
                        Loop temp = (Loop)activeComponent;
                        activeComponent.running = false;
                        if (temp.light_head)
                        {
                            temp.light_head = false;
                            if (temp.before_Child != null)
                            {
                                activeComponent = temp.before_Child.First_Of();
                            }
                            aboutToRunComponent(activeComponent);
                        }
                        else // we are running the diamond
                        {
                            string str = temp.text_str;
                            if (temp.text_str == "")
                            {
                                throw new Exception("Loop not instantiated");
                            }
                            Lexer l = new Lexer(str);
                            if (temp.parse_tree != null)
                            {
                                Boolean_Expression r = (Boolean_Expression)temp.parse_tree;
                                bool rel = r.Execute(l);
                                parentComponent = temp;
                                if (Component.reverse_loop_logic)
                                {
                                    rel = !rel;
                                }

                                if (rel) // evaluate the diamond
                                {
                                    goToNextComponent();
                                }
                                else
                                {
                                    if (temp.after_Child != null)
                                    {
                                        activeComponent = temp.after_Child.First_Of();
                                    }
                                    else // stay here
                                    {
                                        temp.light_head = true;
                                    }
                                    aboutToRunComponent(activeComponent);
                                }
                            }
                        }
                    }
                    else if (activeComponent.GetType() == typeof(Parallelogram))
                    {
                        Parallelogram temp = (Parallelogram)activeComponent;
                        if (temp.is_input)
                        {
                            string str = temp.text_str;
                            if (str == "")
                            {
                                throw new Exception("Input not instantiated");
                            }
                            if (temp.parse_tree != null)
                            {
                                Input inp = (Input)temp.parse_tree;

                                await Dispatcher.UIThread.InvokeAsync(async () =>
                                {
                                    if (myTimer != null)
                                    {
                                        myTimer.Stop();
                                    }
                                    numbers.value v;
                                    if (temp.input_is_expression)
                                    {
                                        Lexer l = new Lexer(temp.prompt);
                                        temp.prompt_result = interpreter_pkg.output_syntax(temp.prompt, false);
                                        temp.prompt_tree = temp.prompt_result.tree;
                                        Expr_Output ex = (Expr_Output)temp.prompt_tree;
                                        v = ex.Execute(l);
                                    }
                                    else
                                    {
                                        v = null; // this really shouldn't happen
                                    }
                                    //numbers.value answer = Runtime.getUserInput(v, temp);

                                    await Runtime.getUserInput(v, temp);
                                    numbers.value answer = temp.pans;
                                    Lexer l2 = new Lexer(temp.Text);
                                    ((Input)temp.parse_tree).Execute(l2, answer);

                                    if (myTimer != null)
                                    {
                                        myTimer.Start();
                                    }
                                });
                            }
                        }
                        else
                        {
                            string str = temp.text_str;
                            if (str == "")
                            {
                                throw new Exception("Output not instantiated");
                            }
                            Lexer l = new Lexer(str);
                            if (temp.parse_tree != null)
                            {
                                Output op = (Output)temp.parse_tree;
                                numbers.value v = op.Execute(l);
                                string outputAns = numbers.Numbers.msstring_view_image(v).Replace("\"", "");
                                if (temp.new_line)
                                {
                                    outputAns += "\n";
                                }
                                Runtime.consoleWrite(outputAns);

                            }

                        }
                        goToNextComponent();

                    }
                }

            }
            catch (Exception e)
            {
                if (myTimer != null)
                {
                    myTimer.Stop();
                }

                Runtime.consoleWriteln("--- Run Halted! ---\n" + e.Message);
            }

        }
        public void OnPauseCommand() {
            if (myTimer == null)
            {
                return;
            }
            MasterConsoleViewModel mc = MasterConsoleViewModel.MC;
            mc.Text += "Run Paused!\n";
            myTimer.Stop();
        }
        
        

        public void Create_Flow_graphx()
        {
            Oval End = new Oval(Visual_Flow_Form.flow_height, Visual_Flow_Form.flow_width, "Oval");
            if (!Component.USMA_mode)
            {
                End.Text = "End";
            }
            else
            {
                End.Text = "Stop";
            }

            this.mainSubchart().Start = new Oval(End, Visual_Flow_Form.flow_height, Visual_Flow_Form.flow_width, "Oval");
            this.mainSubchart().Start.Text = "Start";
            this.mainSubchart().Start.scale = this.scale;
            this.mainSubchart().Start.Scale(this.scale);

            this.Clear_Undo();
            this.modified = false;

            //flow_panel.Invalidate();
        }

        private async Task Save_Before_Losing()
        {
            if (this.modified)
            {

                string msg = "Choosing this option will delete the current flow chart!" + '\n' + "Do you want to save first?";


                    await MessageBoxClass.Show(msg, "Open New Chart", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                    if (buttonAnswer == ButtonResult.Yes)
                    {
                        await OnSaveCommand();
                        checkSave = false;
                    }
                    else if (buttonAnswer == ButtonResult.No)
                    {
                        checkSave = false;
                    }
                    else
                    {
                        checkSave = false;
                    }

            }
            else
            {
                checkSave = false;
            }

        }

        public ButtonResult buttonAnswer = new ButtonResult();
        public bool checkSave = true;

        public async Task OnNewCommand()
        {
            //var msBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
            //    .GetMessageBoxStandardWindow(new MessageBoxStandardParams
            //    {
            //        ButtonDefinitions = ButtonEnum.OkAbort,
            //        ContentTitle = "Title",
            //        ContentMessage = "Message",
            //        Icon = Icon.Plus,
            //        Style = Style.Windows
            //    });
            //msBoxStandardWindow.Show();

            checkSave = modified;

            await Save_Before_Losing();
            //await Dispatcher.UIThread.InvokeAsync(async () => { await Save_Before_Losing(); });

            if (checkSave)
            {
                return;
            }
            Clear_Subcharts();
            Create_Flow_graphx();
            Runtime.Clear_Variables();
            Component.compiled_flowchart = false;
            this.runningState = false;
            this.mainSubchart().Start.scale = this.scale;
            this.mainSubchart().Start.Scale(this.scale);
            //mainSubchart().flow_panel.Invalidate();

            Undo_Stack.Clear_Undo();
            this.Text = My_Title + "- Untitled";
            MainWindow.setMainTitle("RAPTOR");

            this.modified = false;
            this.fileName = null;
            this.file_guid = System.Guid.NewGuid();
            MasterConsoleViewModel.MC.clear_txt();
            this.Update_View_Variables();

        }
        public void OnResetExecuteCommand() {
            OnResetCommand();
            OnExecuteCommand();
        }

        public int executeSpeed = 70;
        public int setSpeed {
            get { return executeSpeed; }
            set {
                this.RaiseAndSetIfChanged(ref executeSpeed, value);
                if (myTimer != null) {
                    myTimer.Interval = 1000 * (1.01 - (double)setSpeed / 100);

                }
            }
        }
        public class myCheckedTimer
        {
            private MainWindowViewModel mvwm;
            private System.Timers.Timer timer;
            public myCheckedTimer(double interval, MainWindowViewModel mvwm)
            {
                this.timer = new System.Timers.Timer(interval);
                this.mvwm = mvwm;
            }
            public bool AutoReset {
                get { return this.timer.AutoReset; }
                set { this.timer.AutoReset = value; }
            }
            public System.Timers.ElapsedEventHandler Elapsed
            {
                set { this.timer.Elapsed += value; }

            }

            public double Interval
            {
                get { return this.timer.Interval; }
                set { this.timer.Interval = value; }
            }
            public void Stop()
            {
                this.timer.Stop();
                mvwm.startTimer = false;
            }
            public void Start()
            {
                this.timer.Start();
            }
        }

        public myCheckedTimer myTimer;
        public void OnExecuteCommand() {
            if (myTimer == null) {

                myTimer = new myCheckedTimer(1000 * (1.01 - (double)setSpeed / 100), this);
                myTimer.AutoReset = false;
                myTimer.Elapsed = new System.Timers.ElapsedEventHandler(stepper);

            } else {
                MasterConsoleViewModel mc = MasterConsoleViewModel.MC;
                mc.Text += "Run Resumed!\n";
            }
            myTimer.Start();
        }

        private Thread InstanceCaller;
        public bool startTimer = false;
        private void stepper(Object source, ElapsedEventArgs e)
        {
            startTimer = true;
            OnNextCommand();
            if (myTimer != null && startTimer)
            {
                myTimer.Start();
            }

            // if (InstanceCaller != null && InstanceCaller.IsAlive)
            // {
            // 	return;
            // }
            // try 
            // {
            // 	InstanceCaller = new Thread(new ThreadStart(this.OnNextCommand));
            //     InstanceCaller.SetApartmentState(ApartmentState.MTA);
            //     InstanceCaller.Priority = ThreadPriority.BelowNormal;
            // 	InstanceCaller.Start();
            // }
            // catch (System.Exception exc)
            // {
            // 	Console.WriteLine(exc.Message);
            // }

        }


        public void OnStepCommand() {
            OnNextCommand();
        }

        public void OnResetCommand() {
            symbolCount = 0;
            if (myTimer != null) {
                myTimer.Stop();
                myTimer = null;
            }
            this.theVariables.Clear();
            if (this.activeComponent == null) {
                return;
            }
            if (this.activeComponent.running) {
                this.activeComponent.running = false;
            }
            foreach(Component c in parentCount)
            {
                if (c.running)
                {
                    c.running = false;
                }
                if (c.selected)
                {
                    c.selected = false;
                }
            }
            this.parentComponent = null;
            this.parentCount = new ObservableCollection<Component>();
            
            this.activeComponent = null;
            inLoop = 0;
            inSelection = 0;
        }

        public void startRun() {
            symbolCount = 0;
            this.theVariables.Clear();
            if (this.activeComponent == null) {
                return;
            }
            if (this.activeComponent.running) {
                this.activeComponent.running = false;
                this.activeComponent = null;
            }
            parentComponent = null;
            parentCount.Clear();
            activeTab = 0;
            activeTabs.Clear();
            activeTabs.Add(0);
            decreaseScope = 0;

        }

        public bool isUndoable = false;
        public bool toggleUndoCommand {
            get { return isUndoable; }
            set { this.RaiseAndSetIfChanged(ref isUndoable, value); }
        }
        public void OnUndoCommand() {
            Undo_Stack.Undo_Action(this.mainSubchart());
        }

        public bool isRedoable = false;
        public bool toggleRedoCommand {
            get { return isRedoable; }
            set { this.RaiseAndSetIfChanged(ref isRedoable, value); }
        }

        // need active tab so we know where to undo and redo changes.
        public int activeTab = 0;
        public ObservableCollection<int> activeTabs = new ObservableCollection<int>() { 0 };

        public int setActiveTab {
            get { return activeTab; }
            set { this.RaiseAndSetIfChanged(ref activeTab, value); }
        }

        public int viewTab = 0;
        public int setViewTab {
            get { return viewTab; }
            set { this.RaiseAndSetIfChanged(ref viewTab, value); }
        }
        public void OnRedoCommand() {
            Undo_Stack.Redo_Action(this.mainSubchart());
        }
        public void OnClearBreakpointsCommand()
        {
            foreach (Subchart s in theTabs)
            {
                s.Start.Clear_Breakpoints();
            }

        }
        public void OnAboutCommand()
        {
            AboutDialog ad = new AboutDialog();
            ad.ShowDialog(MainWindow.topWindow);
        }
        public void OnShowLogCommand() {
            if (log != null)
            {
                Runtime.consoleWrite(log.Display(file_guid, true) + "\n");
            }
            else
            {
                Runtime.consoleWriteln("Log unavailable");
            }

        }
        public void OnCountSymbolsCommand() {
            int count = 0;
            foreach (Subchart s in theTabs)
            {
                Component temp = s.Start;
                while (temp != null)
                {
                    count++;
                    if (temp.GetType() == typeof(Loop))
                    {
                        Loop tempLoop = (Loop)temp;
                        Component temp2;
                        if (tempLoop.before_Child != null)
                        {
                            temp2 = tempLoop.before_Child;
                            while (temp2 != null)
                            {
                                count++;
                                temp2 = temp2.Successor;
                            }
                        }
                        if (tempLoop.after_Child != null)
                        {
                            temp2 = tempLoop.after_Child;
                            while (temp2 != null)
                            {
                                count++;
                                temp2 = temp2.Successor;
                            }
                        }
                    }
                    if (temp.GetType() == typeof(IF_Control))
                    {
                        IF_Control tempIF = (IF_Control)temp;
                        Component temp2;
                        if (tempIF.left_Child != null)
                        {
                            temp2 = tempIF.left_Child;
                            while (temp2 != null)
                            {
                                count++;
                                temp2 = temp2.Successor;
                            }
                        }
                        if (tempIF.right_Child != null)
                        {
                            temp2 = tempIF.right_Child;
                            while (temp2 != null)
                            {
                                count++;
                                temp2 = temp2.Successor;
                            }
                        }
                    }
                    temp = temp.Successor;
                }
            }

            Runtime.consoleWriteln("The total number of symbols is: " + count);

        }

        public void OnGeneralHelpCommand()
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://raptor.martincarlisle.com/docs",
                UseShellExecute = true
            });
        }

        public async void OnRunCompiledCommand() {

            await this.FileSave_Click();
            this.OnResetCommand();

            //Compile_Helpers.Compile_Flowchart(theTabs);
            //Compile_Helpers.Run_Compiled(false);

            try
            {
                Compile_Helpers.Compile_Flowchart(theTabs);
                try
                {
                    Compile_Helpers.Run_Compiled(false);
                }
                catch (System.Exception exc)
                {
                    MessageBoxClass.Show("Flowchart terminated abnormally\n" +
                        exc.ToString());
                }
            }
            catch (System.Exception exc)
            {
                await MessageBoxClass.Show(exc.Message + "\n", "Compilation error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            Component.run_compiled_flowchart = false;
            //this.rescale_all(this.scale);

        }

        public float currentScale = 100;

        public float setCurrentScale {
            get { return currentScale; }
            set {
                this.RaiseAndSetIfChanged(ref currentScale, ((float)value) / 100);
                setAllScales(currentScale);
                setCurrentScaleFormatted = value + "%";
            }
        }

        public string currentScaleFormatted = "100%";

        public string setCurrentScaleFormatted {
            get { return currentScaleFormatted; }
            set { this.RaiseAndSetIfChanged(ref currentScaleFormatted, value); }
        }

        private void setAllScales(float f) {
            this.scale = f;
            foreach (Subchart s in theTabs) {
                s.Start.scale = f;
                s.Start.Scale(f);
            }
        }

        public void setZoom40() {
            setCurrentScale = 40;
        }
        public void setZoom60() {
            setCurrentScale = 60;
        }
        public void setZoom80() {
            setCurrentScale = 80;
        }
        public void setZoom100() {
            setCurrentScale = 100;
        }
        public void setZoom125() {
            setCurrentScale = 125;
        }
        public void setZoom150() {
            setCurrentScale = 150;
        }
        public void setZoom175() {
            setCurrentScale = 175;
        }
        public void setZoom200() {
            setCurrentScale = 200;
        }

        public ObservableCollection<int> ZoomScales = new ObservableCollection<int>(){
            40,
            60,
            80,
            100,
            125,
            150,
            175,
            200
        };



        public ObservableCollection<MenuItemViewModel> GenerateMenuItems = new ObservableCollection<MenuItemViewModel>();

        public ObservableCollection<MenuItemViewModel> getGenerateMenuItems
        {
            get
            {
                return GenerateMenuItems;
            }
        }

    }
}
