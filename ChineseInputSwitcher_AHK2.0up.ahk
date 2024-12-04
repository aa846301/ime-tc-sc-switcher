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
ShowOSD(text) {
    static osd := Gui()
    static isAnimating := false
    static alpha := 255
    
    ; 如果正在動畫中，先清除之前的計時器和GUI
    if (isAnimating) {
        SetTimer(fadeOut, 0)  ; 停止之前的淡出計時器
        isAnimating := false
        alpha := 255  ; 重置 alpha 值
    }
    
    osd.Destroy()  ; 清除舊的 GUI
    osd := Gui("-Caption +AlwaysOnTop +ToolWindow")
    osd.MarginX := 20  ; 增加左右邊距
    osd.MarginY := 10  ; 增加上下邊距
    osd.SetFont("s20 bold", "Microsoft JhengHei")  ; 設置微軟正黑體粗體
    osd.Add("Text", "c0xFFFFFF",  text)  ; 設置白色文字
    
    ; 設置圓角和半透明效果
    osd.BackColor := "222222"  ; 深灰背景
    
    ; 顯示在螢幕中上方
    osd.Show("NoActivate y100")
    
    ; 應用圓角 (Windows 11 style)
    DWMWCP_ROUND := 2  ; 圓角值
    DWMWA_WINDOW_CORNER_PREFERENCE := 33  ; ��角屬性
    DllCall("dwmapi\DwmSetWindowAttribute", "Ptr", osd.Hwnd, "Int", DWMWA_WINDOW_CORNER_PREFERENCE, "Int*", &DWMWCP_ROUND, "Int", 4)
    
    ; 設置初始透明度
    WinSetTransparent(255, osd)
    
    ; 淡出效果
    fadeOut() {
        try {
            if (WinExist("ahk_id " osd.Hwnd)) {
                alpha -= 5  ; 每次減少的透明度
                if (alpha > 0) {
                    WinSetTransparent(alpha, osd)
                    SetTimer(fadeOut, -20)  ; 每20ms執行一次
                } else {
                    osd.Destroy()
                    alpha := 255  ; 重置 alpha 值為下次使用
                    isAnimating := false
                }
            }
        } catch {
            alpha := 255  ; 重置 alpha 值
            isAnimating := false
        }
    }
    
    ; 500毫秒後開始淡出
    isAnimating := true
    SetTimer(() => SetTimer(fadeOut, -20), -500)
}

; 獲取窗口所在的螢幕信息
MonitorFromWindow(hwnd) {
    ; 獲取窗口位置
    try {
        windowPos := WinGetPos(hwnd)
    } catch {
        ; 如果獲取失敗，返回主螢幕信息
        return {left: 0, top: 0, right: A_ScreenWidth, bottom: A_ScreenHeight}
    }
    
    ; 獲取所有螢幕信息
    monitorCount := MonitorGetCount()
    
    Loop monitorCount {
        monitorInfo := MonitorGet(A_Index)
        if (windowPos.x >= monitorInfo.left && windowPos.x < monitorInfo.right
            && windowPos.y >= monitorInfo.top && windowPos.y < monitorInfo.bottom) {
            return monitorInfo
        }
    }
    
    ; 如果沒找到，返回主螢幕信息
    return MonitorGet(MonitorGetPrimary())
}

; 獲取窗口大小
GetWindowSize(guiObj) {
    try {
        hwnd := guiObj.Hwnd  ; 先獲取 Hwnd
        pos := WinGetPos("ahk_id " hwnd)  ; 使用字符串形式的窗口標識符
        return {width: pos.width, height: pos.height}
    } catch {
        return {width: 0, height: 0}
    }
}

^+f::
{
    try 
    {
        ; 保存當前視窗句柄
        currentWindow := WinExist("A")
        
        ; 嘗試讀取註冊表值
        currentValue := RegRead("HKCU\" regPath, valueName)
        
        ; 如果目前是簡體("1")，切換到繁體("0")
        if (currentValue = "0x00000001")
        {
            RegWrite("0x00000000", "REG_SZ", "HKCU\" regPath, valueName)
            ; 切換到任務欄然後切回來
            WinActivate("ahk_class Shell_TrayWnd")
            Sleep(50)
            WinActivate("ahk_id " currentWindow)
            if (IsOpenTrayTip)
            {
                ShowOSD("繁體")
            }
        }
        ; 如果目前是繁體("0")，切換到簡體("1")
        else if (currentValue = "0x00000000")
        {
            RegWrite("0x00000001", "REG_SZ", "HKCU\" regPath, valueName)
            ; 切換到任務欄然後切回來
            WinActivate("ahk_class Shell_TrayWnd")
            Sleep(50)
            WinActivate("ahk_id " currentWindow)
            if (IsOpenTrayTip)
            {
                ShowOSD("簡體")
            }
        }
    }
    catch as err
    {
        ; 如果讀取失敗，顯示錯誤訊息
        MsgBox("錯誤輸入法註冊表項目不存在`n" err.Message, "錯誤", "Icon!")
    }
    
    return
}