#Requires AutoHotkey v2.0

; 定義註冊表路徑
regPath := "SOFTWARE\Microsoft\IME\15.0\IMETC"
valueName := "Enable Simplified Chinese Output"

; 開關通知功能
IsOpenTrayTip := true

; 首次開啟時跳出說明視窗

MsgBox("
(
使用說明:
快捷鍵:Ctrl+Shift+F
開關通知:Ctrl+Shift+Alt+X
注意!若切換後無效果，請手動切換其他輸入法再切回中文輸入法
)","ime-tc-sc-switcher")

^+!x::
{
	global IsOpenTrayTip  ; 使用 global 關鍵字來引用全域變數
	IsOpenTrayTip := !IsOpenTrayTip
	TrayTip("通知開關", "通知已切換")
}

; 定義快捷鍵 Ctrl+Shift+F
^+f::
{
    try 
    {
        ; 嘗試讀取註冊表值
        currentValue := RegRead("HKCU\" regPath, valueName)
        
        ; 如果目前是簡體("1")，切換到繁體("0")
        if (currentValue = "0x00000001")
        {
            RegWrite("0x00000000", "REG_SZ", "HKCU\" regPath, valueName)
            if (IsOpenTrayTip)
			{
				TrayTip("輸入法切換", "已切換至繁體輸出模式")
			}
        }
        ; 如果目前是繁體("0")，切換到簡體("1")
        else if (currentValue = "0x00000000")
        {
            RegWrite("0x00000001", "REG_SZ", "HKCU\" regPath, valueName)
			if (IsOpenTrayTip)
			{
				TrayTip("輸入法切換", "已切換至簡體輸出模式")
			}
        }
    }
    catch as err
    {
        ; 如果讀取失敗，顯示錯誤訊息
        MsgBox("錯誤：輸入法註冊表項目不存在`n" err.Message, "錯誤", "Icon!")
    }
    
    return
}