   M S B A M L   `   `   `  ���� W  SFormiik.DependenciesAnalyzer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:Formiik.DependenciesAnalyzerFormiik.DependenciesAnalyzer  7    1Formiik.DependenciesAnalyzer.DependenciesAnalyzer   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     <local*clr-namespace:Formiik.DependenciesAnalyzer     U QMaterialDesignThemes.Wpf, Version=2.3.1.953, Culture=neutral, PublicKeyToken=nullFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  -     � Title$&   Dependencies Analyzer by Formiik��$	��400��$	��600�� � WindowState$ 	Maximized=�   MaterialDesignBody#���   $��Regular"�$��13(�&   System.Windows.Media.TextOptions   TextFormattingMode$ Ideal=�   TextRenderingMode$
 Auto=�  MaterialDesignPaper#���    MaterialDesignFont#���   � Icon$ Images\monitor.ico��.���  � RowDefinitions �� $
��auto ��� $��* �.+�+  %System.Windows.Controls.Ribbon.Ribbon  -     Name$ 
RibbonTool�� �� SelectedIndex$ 0��   ApplicationMenu :  4System.Windows.Controls.Ribbon.RibbonApplicationMenu  5  /System.Windows.Controls.Ribbon.RibbonMenuButton	   KeyTip$	 F��
   LargeImageSource$
 Images\large\app.png��   SmallImageSource$ Images\small\app.png��.@�>  8System.Windows.Controls.Ribbon.RibbonApplicationMenuItem  -   $
��Exit��3  -System.Windows.Controls.Ribbon.RibbonMenuItem   ImageSource$ Images\exit.png��.@�.  (System.Windows.Controls.Ribbon.RibbonTab  $��Configuration��   KeyTip$ C��.@�0  *System.Windows.Controls.Ribbon.RibbonGroup  -     Name$ Clone��$��	Git Clone��.@��  �� $��* �.+�1	  +System.Windows.Controls.Ribbon.RibbonButton	  -    	 Name$ btnSetRepository�� 	  Label$ Set Remote Repository��$��Center=� 	  LargeImageSource$, &Images\large\set-remote-repository.png�� 	  SmallImageSource$, &Images\small\set-remote-repository.png��  -   $	 Git��$��Git Repository��.@��  � ColumnDefinitions �� $��* � �� $��* �.+�	  -   $ btnSeleccionarRepo��$��2��$��2��$ Set Work Directory��$��Center=�$% Images\large\work-directory.png��$% Images\small\work-directory.png��  -   $ RemoteBranches��$)��#Remote Branches from Git Repository��.@��  �� $��* ��� $��* ��� $��* � �� $��* �.+��� .+��� .��Select a Branch:3
  -System.Windows.Controls.Ribbon.RibbonComboBox
  -	    
 Name$ rbncmbRemoteBranches�� 
  SelectionBoxWidth$	 200Q�$��Center=� �� IsTextSearchEnabled .   �� IsTextSearchCaseSensitive .  .@�2  ,System.Windows.Controls.Ribbon.RibbonGallery  -
     Name$ remoteBranchesGallery��$	��200��.@�:  4System.Windows.Controls.Ribbon.RibbonGalleryCategory  -     Name$ remoteBranchesCategory��	  -   $ 
btnRefresh��$��1��$��	10,10,0,0q�$ Refresh��$��Center=�$ Images\large\refresh.png��$ Images\small\refresh.png��	  -   $ btnFetchCheckout��$��2��$��	10,10,0,0q�$ Checkout��$��Center=�$ Images\large\checkout.png��$ Images\small\checkout.png��  -   $
 Pull��$
��Pull��.@�	  -   $ btnPull��$ Pull Changes From Server��$��Center=�$ Images\large\pull.png��$ Images\small\pull.png��  -   $ Analyze��$��Analyze��.@�	  -   $ 
btnAnalyze��$ Start Analysis��$��Center=�$ Images\large\start.png��$ Images\small\start.png��	  -   $ btnStopAnalysis��$ Stop Analysis��$��Center=�$ Images\large\stop.png��$ Images\small\stop.png��  -   $
 Scan��$
��Scan��.@�	  -   $ btnScan��$ Scan All Components�� � ToolTip$D >Scan the solution for to found all components that can affect.��$��Center=�$ Images\large\scan.png��$ Images\small\scan.png��]� $��1��.+��� $
��Left=�$	��200��
�������.+��� $
��Bold"�$��10,0,0,0q�.��Path Git Executable:�� -    ��Name$ 
lblPathGit��$��WrapWithOverflow=�$��	10,0,0,10q��� $
��Bold"�$��10,0,0,0q�.��Url Remote Repository:�� -   $ lblRemoteRepoUrl��$��WrapWithOverflow=�$��	10,0,0,10q��� $
��Bold"�$��10,0,0,0q�.��User:�� -   $ lblUserRemoteRepo��$��WrapWithOverflow=�$��	10,0,0,10q��� $
��Bold"�$��10,0,0,0q�.��	Password:�� $��10,0,0,0q�.��
********�� $
��Bold"�$��10,0,0,0q�.��Local Repository:�� -    ��Name$ lblPathRepo��$��	10,0,0,10q��� $��Bottom=����� .R��  �� $��* ��� $
��auto ��� $��* ��� $
��auto ��� $��* ��� $
��auto ��� $��* ��� $
��auto ��� $��* ��� $
��auto ��� $��* ��� $
��auto ��� $��* ��� $
��auto ��� $��* ��� $
��auto ��� $��* �.@��� .��� -    �Name$ pgbIndeterminate��$	��140��$��15���� $��1���� $��2��.���� -   $ 
txtRenamed���� $��3���� $��4��.���� -   $ txtModified���� $��5���� $��6��.���� -   $ txtAdded���� $��7���� $��8��.���� -   $ 
txtDeleted���� $��9���� $��10��.���� -   $ 	txtCopied���� $��11���� $��12��.���� -   $ txtUpdateButUnmerged���� $��13���� $��14��.���� -     ��Name$
 Less��.��Zoom -�� $��15���� $��16��.���� -!   $
 More��.��Zoom +�  �� $
��0.3* ��� $��4 ��� $��* �.+��� $��0��$��Visible=�$��Visible=�.���� .+��� -"   $ lblArchivosModificados��$
��Bold"�R� -#    R�Name$ treeViewRoot��$��0,0,0,0q��� $��1��$��5��$��Stretch=�� $��2�� �� $��* ��� $��4 ��� $��* ��� $��4 ��� $��* � �� $��* �.+�� $��0�� �� $
��auto ��� $��* �.+�� $��0�� �� $��* ��� $��* �.+��� -$   $ lblTotalComponents��$��0��$
��Bold"�$��5q��� -%   $! btnExportarTextoComponentes��$��1��$
��Bold"�$��5q�$y�	Collapsed=�$��5q�.��Export Text�� $��1��$��Visible=�$��Visible=�.���� -&     ��Name$  stackAllComponents��! �� Orientation$! Vertical=��� $��1��$��4��$��Stretch=�� $��2�� �� $
��auto ��� $��* �.+�� $��0�� �� $��* ��� $��* �.+��� -'   $ lblModulosAfectados��$��0��$
��Bold"�$��5q��� -(   $ btnExportarTextoAfectados��$��1��$
��Bold"�$��5q�$y�	Collapsed=�$��5q�.��Export Text�� $��1��$��Visible=�$��Visible=�.���� -)   $  stackModules��$! Vertical=��� $��3��$��5��$��Stretch=�� -*   " �Name$" mainGrid��$��4��