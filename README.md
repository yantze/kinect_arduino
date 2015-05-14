# final project: kinect arduino

这个项目是我大学的毕业设计，时间仓促，略有潦草

## 项目说明

- basic 基于基本的Kinect结构写的colorImage
- interaction 交互，主要是按钮的push和滑动操作
- SkeletonInDepth 深度和骨架数据重叠
- HandTrackingFramework 手势识别，共八个基本手势

## 成果展示
[打开新页面播放视频](http://v.youku.com/v_show/id_XOTU1MDI2MzY0.html)
<!--
<object classid="clsid:D27CDB6E-AE6D-11cf-96B8-444553540000"
    codebase="http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=6,0,29,0"
    height="20" width="20"> 
<param name="movie" value="http://raw.github.com/yantze/kinect_arduino/master/doc/vcastr22.swf?vcastr_file=http://raw.github.com/yantze/kinect_arduino/master/doc/2466.flv"> 
<param name="quality" value="high"> 
<param name="allowFullScreen" value="true" /> 
<embed src="http://raw.github.com/yantze/kinect_arduino/master/doc/vcastr22.swf?vcastr_file=http://raw.github.com/yantze/kinect_arduino/master/doc/2466.flv"
    pluginspage="http://www.macromedia.com/go/getflashplayer"
    type="application/x-shockwave-flash"
    quality="high" width="800" height="450"> 
</embed> 
</object>
-->

## 视频说明

1. 先左右上下控制机械手臂左右上下运动
2. 到了27秒左右，使用T型姿势，使机械手臂停止交互运动
3. 手势识别
    1. 右手左右挥动三次到四次，电脑屏幕显示为右挥动
    2. 左手左右挥动三次到四次，电脑屏幕显示为左挥动
    3. 右手向左臂方向滑动，电脑屏幕显示为左滑
    4. 左手向右臂方向滑动，电脑屏幕显示为右滑
4. 执行T姿势，重新启动并控制机械手臂，使其慢速左右和上下

[下载演示视频](http://raw.github.com/yantze/kinect_arduino/master/doc/2466.flv)


