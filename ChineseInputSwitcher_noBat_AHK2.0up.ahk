#Requires AutoHotkey v2.0

; 定義註冊表路徑
regPath := "SOFTWARE\Microsoft\IME\15.0\IMETC"
valueName := "Enable Simplified Chinese Output"

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
            ; MsgBox("已切換至繁體輸出模式")
			TrayTip("輸入法切換", "已切換至繁體輸出模式")
        }
        ; 如果目前是繁體("0")，切換到簡體("1")
        else if (currentValue = "0x00000000")
        {
            RegWrite("0x00000001", "REG_SZ", "HKCU\" regPath, valueName)
            ; MsgBox("已切換至簡體輸出模式")
			TrayTip("輸入法切換", "已切換至簡體輸出模式")
        }
    }
    catch as err
    {
        ; 如果讀取失敗，顯示錯誤訊息
        MsgBox("錯誤：輸入法註冊表項目不存在`n" err.Message, "錯誤", "Icon!")
    }
    
    return
}