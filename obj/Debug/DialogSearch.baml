   M S B A M L   `   `   `  ����T  PMahApps.Metro, Version=1.2.4.0, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4fV;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.MetroMahApps.Metro.Controls  5   	   K GLRFPacketDecoder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:LRFPacketDecoderLRFPacketDecoder 5   	   #   LRFPacketDecoder.DialogSearch   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   PControls;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro     5   	   0localclr-namespace:LRFPacketDecoder     5   	   X TXceed.Wpf.Toolkit, Version=2.9.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  5
      ���	#FF292929(   "MahApps.Metro.Controls.MetroWindow    TitleForeground  �	#FFB4B4B4 � WindowStartupLocation$ CenterOwner=�5   <      	GlowBrush �	#FF007ACC � Title$ 
Search Box��5   	   $	��387��6   $	��524��6)   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��30 �6   �� 5      $��* �6   �� 5      $��40 �6    � ColumnDefinitions 5   
   �� 5      �� 5      �� 5      .+�5   
   �� -   5        $��	txtsearch��5      $��5q�6   $��1��6.   $��3��6;   6O   �� 5   
   $��0��6   $��3��6"    �� Orientation$ Vertical=�66   .+�5      �� $ 
Horizontal=�6   $��5q�62   .+�5      �� $��FirstSub Playet NetID��6   ���	#FFC5C5C5$��Center=�6P   $	��150��6k   �� 5       -   5        $��firstPlayers��5       $	��300��6/   ��5!      �� 5"      .R�5#      �  5$   "   �� 5%   &   $��* �67   �� 5&   &   $��* �67   .+�5(   "   �� $��0��6B   ��6,   �� *name+6B   �� 5)   "   $��1��6F   ��6,   �� *
champion+6F   5*      5+      5-      �� 5.      -   5        $��X��5.      $��35��6%   $��10,0,0,0q�60   6B   5/      �� 50      $ 
Horizontal=�6   $��5q�62   .+�51      �� $��Contains Player NetID��6   ���	#FFC5C5C5$��Center=�6P   $	��150��6k   �� 52      -   5        $��containPlayers��52      $	��300��61   ��53      �� 54      .R�55      �  56   "   �� 57   &   $��* �67   �� 58   &   $��* �67   .+�5:   "   �� $��0��6B   ��6,   �� *name+6B   �� 5;   "   $��1��6F   ��6,   �� *
champion+6F   5<      5=      5?      �� 5@      -   5        $��X��5@      $��35��6%   $��10,0,0,0q�60   6B   5A      �� 5B      $ 
Horizontal=�6   $��5q�62   .+�5C      �� $��
Time Range��6   ���	#FFC5C5C5$��Center=�6E   $	��150��6`   �� 5D      -   5        $��chTime��5D      &   Xceed.Wpf.Toolkit.TimeSpanUpDown  5E      -   5        $��	startTime��5E   &   �  �Xceed.Wpf.Toolkit.Primitives.UpDownBase`1[[System.Nullable`1[[System.TimeSpan, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]   Value$ 00:00:00X�67   
�������$	��100��6[   ���	#FF3F3F46���	#FF3333376~     5F      -   5        $��endTime��5F   &   $ 00:00:00X�65   
�������$	��100��6Y   ���	#FF3F3F46���	#FF333337$��10,0,0,0q�6�   5G      5H      �� 5K   
   -	   5        $
��list��5K   N   $��5q�6   $��2��6   $��3��6*   $��0��6>    w� TextPath$
 Text��6Z   	 �� IsTextSearchEnabled	 . 
 %� IsVirtualizing
 .  ��5L      �� 5M      .R�5N      �� $	��ch1��6   ���	#FFF5F3F3��6A   �� *Text+5O      5Q      �� 5S   
   -
   5         ��Name$ 	allselect��5S      $��5q�6   $��3��6/   $��1��6<   $��Hepsini Seç��6L   6b   �� 5T   
   -   5        $ cancel��5T      $��5q�6   $��3��6-   $��0��6:   $��İptal��6J   6Z   �� 5U   
   -   5        $
 okey��5U      $��5q�6   $��3��6*   $��2��67   $��Tamam��6G   6W   5V      5W      