   M S B A M L   `   `   `  ����  Test    Test.Splash   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$   ��5   	   $	��250��6   $	��350��6    � WindowStartupLocation$ CenterScreen=�6+    � WindowStyle$
 None=�5   	   
�������.��5      �  � RowDefinitions 5   
   �� 5	      $��* �6   �� 5
      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   � $��0��6   $��Center=�6   $��Center=�69   .+�5      A� $��60��6   $��60��6!   $��6��6-   ���#1f2a4b J� RenderTransformOrigin$ 0.5,0.5&�6R    �� StrokeDashArray 5      S�1 3R�6,   z�5      �� 5      -   5        circleRotateTransform�� �� Angle$ 0Q�5   E    � Triggers 5      :� 5       :� RoutedEvent$ Loaded��6#   .[�5      �� .��5      �� 	 g� RepeatBehavior$	 Forever��6)   .��5   "   W� 
 �� 
TargetName$
 circleRotateTransform��62    �� TargetProperty$ Angle�5   2    W� From$ 0X�5   2   	 W� To$	 360X�6;    g� Duration$ 0:0:1D�6D   5      5      5      5!      A� 5"      $��40��6   $��40��6!   $��4��6-   
������ $��Center=�6V   $��Center=�6s   ��5#      
� 5$       
� GradientOrigin$ 0.5,0.5&�6*    
� Center$ 0.5,0.5&�6C    
� RadiusX$	 0.5Q�6T    
� RadiusY$	 0.5Q�6b   .��5%      �  � Offset$ 0Q�6'    � Color$ #1f2a4b��62   � 5&      $ 1Q�6'   $ 	#00000000��62   5'      z�5)      �� 5*      -   5        innerCircleRotateTransform��$ 0Q�5*   J    5,      :� 5-      $ Loaded��6#   .[�5.      �� .��5/      �� $	 Forever��6)   .��50   "   W� $ 
 innerCircleRotateTransform��62   $ Angle�51   2   $ 0X�52   2   $
 -360X�6;   $ 0:0:1D�6E   53      54      55      57      58      �� 5:   
   $��1��6   $��
Loading...��6!   $��16(�63   ���#1f2b4a$��Center=�6V   �� 5<   
   $��2��6   $��© RR DONNELLEY��6!   $��12(�67   ���#1f2b4a$��Center=�6Z   $
��Bold"�6w   5=      5>      