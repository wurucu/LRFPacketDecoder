   M S B A M L   `   `   `  ����T  PMahApps.Metro, Version=1.2.4.0, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4fV;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.MetroMahApps.Metro.Controls  5   	   K GLRFPacketDecoder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullE'clr-namespace:LRFPacketDecoder.ControlsLRFPacketDecoder.Controls 5   	   3clr-namespace:LRFPacketDecoderLRFPacketDecoder 5   	   !   LRFPacketDecoder.MainWindow   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   PControls;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro     5   	   7wct'clr-namespace:LRFPacketDecoder.Controls     5   	   0localclr-namespace:LRFPacketDecoder     5   	   -   5        (   "MahApps.Metro.Controls.MetroWindow    TitleForeground  �	#FFB4B4B4   EnableDWMDropShadow .  � 
ResizeMode$ CanResizeWithGrip=�5   	      WindowTransitionsEnabled .   � Title$ Packet Decode��5   	   $	��600��6   $
��1100��6,   ���	#FF007ACC   RightWindowCommands 5      +   %MahApps.Metro.Controls.WindowCommands  5   
   .@�5      �� -   5        $��
Dosya Seç��5      6)   �� 5      -   5        $��Filtrele��5      6(   �� 5      -   5        $��Kullanıcılar��5      6,   5      .��5      �  � RowDefinitions 5   
   �� 5      $��0 �6   �� 5      $��* �6   �� 5      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      $	��330 �6   �� 5       $��3 �6   �� 5!      $��* �6   �� 5"      $��3 �6   .+�5%   
   � $��0��6   $��1��6   .+�5&      &   System.Windows.Controls.DataGrid  -   5          Name$ 
packetList��5&   3   	   AutoGenerateColumns	 .  
   SelectionMode$
 Extended=�6G      
IsReadOnly .    SelectionUnit$ FullRow=�6r      Columns 5'      0  *System.Windows.Controls.DataGridTextColumn  5(      ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$ 80 6)      Header$
 Time��64   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6B   �� *STime+6^     5)      $ * 6)   $
 Name��63    6A   �� *SHead+6]     5*      $ 70 6)   $
 Size��64    6B   �� *Length+6_    � 	Resources 5-      )  #System.Windows.Controls.DataGridRow �� 
TargetType��%E   '        �� 5.      "  6   .�5/      -   5        50      51      52      54      �� 57   
   $��1��6   $��1��6$   $��3��64   $��Stretch=�6>   ���#2d2d306\   �� 59   
   -   5         ��Name$ 
tabControl��59   2   $��1��6   $��2��6"   ���	#FF8B5D5D5;      -	  'LRFPacketDecoder.Controls.WLoadingPanel	  5=   
   -   5         	 Name$ loading��5=      $��3��6-   $��4��6:   $��Bottom=�6N   $	��100��6i   $y�	Collapsed=�6v   5?      5B      5C      