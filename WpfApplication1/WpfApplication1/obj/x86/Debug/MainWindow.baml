   M S B A M L   `   `   `  ����  WpfApplication1     WpfApplication1.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   W   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     6�   -   5          � Title$!  Cellular Autamata Simulator��5   	    � SizeToContent$ WidthAndHeight=�6  .��5      ��  �� Orientation$ 
Horizontal=�6   $��Center=�6*   $��Center=�6G   .+�5   
   �� $ Vertical=�6   $��
12,12,0,12q�6,   .+�5      �� $ 
Horizontal=�6   .+�5      �� -   5        $��
caScroller��5      $��Hidden=�61   $��Hidden=�6X   $	��500��6}   $	��500��6�   .��5	      �� -   5        $��caImage��5	   N    �� BitmapScalingMode$ NearestNeighbor=�6   $	��500��6]   $	��500��6i   6�   5
      �� 5      -   5        $��vScroll��5       � Maximum$	 500Q�6+    � LargeChange$ 10Q�69    � SmallChange$ 1Q�6n   5      �� 5      -   5        $��hScroll��5       �� Orientation$ 
Horizontal=�6'   $	 500Q�6@   $ 10Q�6N   $ 1Q�6�   5      �� 5   
   -   5        $��stackPanel1��5   Y   $ Vertical=�6   $��Right=�6,   $��4,4,4,4q�6H   $��Bottom=�6l   .+�5      �� -   5        $��caBox��5      $��0,0,0,5q�6&   .+�5      �� $��Current State:��6   �� 5      -   5        $��curCA��5      $��UnInited��6%   $��20,0,0,0q�68   5      �� 5      -	   5        $��	caLoadBox��5      $��0,0,0,5q�6*   .+�5      �� $��Current CA:��6   �� 5      $ 
Horizontal=�6   $��Center=�66   .+�5      �� -
   5        $��caLoad��5      $
��Load��6+   $��Center=�6:   6U   �� 5      -   5        $��caCreate��5      $��Create��6-   $��Center=�6>   6Y   5      5      �� 5      -   5        $��stateBox��5      $��0,0,0,5q�6)   .+�5      �� $��Grid State:��6   �� 5      $ 
Horizontal=�6   $��Center=�66   .+�5      �� -   5        $��	stateLoad��5      $
��Load��6.   $��Center=�6=   6X   �� 5      -   5        $��	stateSave��5      $
��Save��6.   $��Center=�6=   6X   �� 5       -   5        $��
stateClear��5       $��Clear��6/   $��Center=�6?   6Z   5!      5"      �� 5#      -   5        $��	statesBox��5#      $��0,0,0,5q�6*   .+�5$      �� $��States��6   �� 5%      -   5        $��listBox1��5%   '   $	��100��6   6'   5&      �� 5'      $ 
Horizontal=�6   .+�5(      �� -   5        $��	zoomLabel��5(   3   $
��Zoom��6   $��28��6'   63   �� 5)      -   5        $��
zoomSlider��5)      $��50��6+   $	 100Q�66    � Minimum$ 1Q�6D   5*      �� 5+      $ 
Horizontal=�6   .+�5,      �� -   5        $��caPlay��5,      $
��Play��6'   $��Center=�66   6Q   �� 5-      -   5        $��caPause��5-      $��Pause��6(   $��Center=�68   6S   �� 5.      -   5        $��caStep��5.      $
��Step��6'   $��Center=�66   6Q   5/      50      52      53      