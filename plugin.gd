@tool
extends EditorPlugin

var dependencies_addons := [
	"https://gitee.com/FengLucky/UniTaskForGodot.git","UniTaskForGodot",
	"https://gitee.com/FengLucky/GodotSharpLog.git","GodotSharpLog",
];
var luban_extend_git_url := "https://gitee.com/FengLucky/luban-extend.git";

var godot_sharp_log_version := "0.1";
var uni_task_version := "0.1"

var menu: PopupMenu

func _enter_tree() -> void:
	menu = PopupMenu.new()
	menu.add_item("安装依赖",0);
	menu.add_item("打表",1);

	# 绑定点击事件
	menu.connect("id_pressed", self._on_menu_id_pressed)
	add_tool_submenu_item("LFFramework",menu)
	
func _exit_tree() -> void:
	remove_tool_menu_item("LFFramework")

func _on_menu_id_pressed(id):
	self._disable_all_menu_item(true);
	match id:
		0:
			await self._install_dependencies();
		1:
			var os_name := OS.get_name();
			if os_name == "Windows":
				await self._execute_with_pipe("powershell.exe",["/c","Config/打表-json.bat"],"|ERROR|")	
				print("打表结束")
			elif os_name == "macOS"	or os_name == "Linux" or os_name == "BSD":
				await self._execute_with_pipe("Config/打表-json.sh",[],"Error")	
				print("打表结束")
			else:
				printerr("Luban不支持的平台:"+os_name);
			
	self._disable_all_menu_item(false);
	
func _disable_all_menu_item(disabled:bool):
	for i in menu.item_count:
		menu.set_item_disabled(i,disabled);	
			
func _install_dependencies():
	if not await self._install_luban():
		printerr("依赖安装失败，请解决错误后通过 [项目]->[工具]->[LFFramework]->[安装依赖] 菜单重试")
		return;
	var i :int = 0;
	while i < dependencies_addons.size():
		var url = dependencies_addons[i];
		var target = dependencies_addons[i+1];
		if not await self._install_addons_from_git(url,target):
			printerr("依赖安装失败，请解决错误后通过 [项目]->[工具]->[LFFramework]->[安装依赖] 菜单重试")
			return;
		i+=2;
		
	get_editor_interface().get_resource_filesystem().scan();	
	print("LFFramework 依赖安装成功!");
		
func _install_luban()->bool:
	if not await self._install_or_update_git_repo(luban_extend_git_url,"Luban"):
		return false;		
	var os_name := OS.get_name();
	if os_name == "Windows":
		await self._execute_with_pipe("powershell.exe",["/c",".godot/git_cache/Luban/build.bat"]);
	elif os_name == "macOS"	or os_name == "Linux" or os_name == "BSD":
		await self._execute_with_pipe(".godot/git_cache/Luban/build.sh",[]);
	else:
		printerr("编译Luban不支持的平台:"+os_name);
		return false;
	
	var dir := DirAccess.open("res://");
	if not dir.dir_exists(".godot/git_cache/Luban/Build/publish"):
		printerr("Luban 编译失败");
		return false;
	
	if not self._deep_delete_directory("Config/Bin") or not self._deep_delete_directory("Config/Templates"):
		return false;
		
	if not self._deep_copy_directory(".godot/git_cache/Luban/Build/publish","Config/Luban"):
		printerr("复制 Luban 二进制文件到 Config/Luaban 失败")
		return false;
		
	if not self._deep_copy_directory(".godot/git_cache/Luban/Templates","Config/Templates"):
		printerr("复制 Luban 模板文件到 Config/Templates 失败")
		return false;
		
	if not self._copy_directory("addons/LFFramework/Config","Config"):
		printerr("复制 Luban 脚本和配置文件到 Config 失败")
		return false;
	
	if not self._deep_copy_directory("addons/LFFramework/Config/Define","Config/Define"):
		printerr("复制 Luban 定义文件到 Config/Define 失败")
		return false;	
	
	if not self._deep_copy_directory("addons/LFFramework/Config/Excel","Config/Excel",dir,false):
		printerr("复制 Luban Excel 文件到 Config/Define 失败")
		return false;	
		
	if not self._deep_copy_directory("addons/LFFramework/Config/Json","Config/Json",dir,false):
		printerr("复制 Luban Json 文件到 Config/Define 失败")
		return false;			
	
	return true;	
		
func _install_addons_from_git(url:String,target:String)->bool:
	if not await self._install_or_update_git_repo(url,target):
		return false;
		
	var dir := DirAccess.open("res://");	
	var full_path := ".godot/git_cache/"+target+"/addons";	
	for directory in dir.get_directories_at(full_path):
		if not self._deep_delete_directory("addons/"+directory):
			return false;

	if not self._deep_copy_directory(full_path,"addons"):
		printerr("复制插件 "+ target +" 到项目失败")
		return false;
		
	return true;	
		
func _install_or_update_git_repo(url:String,target:String)->bool:
	var full_path := ".godot/git_cache/"+target;
	var dir := DirAccess.open("res://");
	if dir.dir_exists(full_path):
		await self._execute_with_pipe("git",["-C",full_path,"pull"],"fatal:");
	else:
		await self._execute_with_pipe("git",["clone","--depth","1",url,full_path],"fatal:")

	if not dir.dir_exists(full_path):
		printerr("git 仓库 "+ target +" 下载失败")
		return false;
	return true;	
	
func _execute_with_pipe(path: String, arguments: PackedStringArray,errFlag:String = "")->bool:
	var output:Dictionary = OS.execute_with_pipe(path,arguments,false);
	if output == null:
		return false;
		
	var stdio :FileAccess = output["stdio"];
	var stderr :FileAccess = output["stderr"];
	var pid :int = output["pid"];

	while true:
		await get_tree().create_timer(0.1).timeout;
		var log := stdio.get_line();
		var err := stderr.get_line();
		if log.length() > 0:
			if log.to_lower().begins_with("press any key"):
				stdio.store_line("\n");
			elif log.contains(errFlag):
				printerr(log);	
			else:
				print(log);
			
		if err.length() > 0:
			if err.contains(errFlag):
				printerr(err);
			else:
				print(err);	
			
		if not OS.is_process_running(pid):
			break;
	return true;
	
func _deep_delete_directory(path: String,dir:DirAccess = null) -> bool:
	if dir == null:
		dir = DirAccess.open("res://");
		if dir == null:
			return true;
	if not dir.dir_exists(path):
		return true;		
	# 遍历目录内容
	for directory in dir.get_directories_at(path):
		if not self._deep_delete_directory(path+"/"+directory,dir):
			return false;
	
	for file in dir.get_files_at(path):
		if dir.remove(path + "/" + file) != OK:
			printerr("删除文件失败:"+(path + "/" + file))
			return false;
	# 最后删除当前目录（现在为空）
	if dir.remove(path) != OK:
		printerr("删除文件夹失败:"+path);
		return false;
	return true;	
	
func _deep_copy_directory(from:String,to:String,dir:DirAccess = null,cover:bool = true) -> bool:
	if dir == null:
		dir = DirAccess.open("res://");
		if dir == null:
			return true;
			
	if not dir.dir_exists(to):
		if dir.make_dir_recursive(to) != OK:
			return false;		
			
	for directory in dir.get_directories_at(from):					
		if not self._deep_copy_directory(from+"/"+directory,to+"/"+directory,dir,cover):
			return false;
	
	return self._copy_directory(from,to,dir,cover);
	
func _copy_directory(from:String,to:String,dir:DirAccess = null,cover:bool = true) -> bool:
	if dir == null:
		dir = DirAccess.open("res://");
		if dir == null:
			return true;
	for file in dir.get_files_at(from):
		if dir.file_exists(to+"/"+file) and not cover:
			continue;
		if dir.copy(from+"/"+file,to+"/"+file) != OK:
			return false;
	return true;		
