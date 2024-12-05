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
快捷鍵: Shift+Alt+F
開關通知: Ctrl+Shift+Alt+X
注意!若切換後無效果，請手動切換其他輸入法再切回中文輸入法
)","ime-tc-sc-switcher")

^+!x::
{
	global IsOpenTrayTip  ; 使用 global 關鍵字來引用全域變數
	IsOpenTrayTip := !IsOpenTrayTip
	
	; 使用相同的 GUI 風格顯示通知
	ShowOSD(IsOpenTrayTip ? "通知已開啟" : "通知已關閉")
}


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
    
    ; 獲取窗口位置和大小 - 修正語法
    hwnd := osd.Hwnd
    local x, y, w, h
    WinGetPos(&x, &y, &w, &h, hwnd)
    
    ; 創建圓角區域
    radius := 10  ; 圓角半徑
    hRgn := DllCall("CreateRoundRectRgn", "Int", 0, "Int", 0
        , "Int", w, "Int", h
        , "Int", radius*2, "Int", radius*2)
    
    ; 應用圓角區域到窗口
    DllCall("SetWindowRgn", "Ptr", hwnd, "Ptr", hRgn, "Int", true)
    
    ; 設置初始透明度
    WinSetTransparent(255, hwnd)
    
    ; 淡出效果
    fadeOut() {
        try {
            if (WinExist(hwnd)) {
                alpha -= 5
                if (alpha > 0) {
                    WinSetTransparent(alpha, hwnd)
                    SetTimer(fadeOut, -20)
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
!+f::
{
    try 
    {
        ; 保存當前視窗句柄
        currentWindow := WinExist("A")
        
        ; 嘗試讀取註冊表值
        currentValue := RegRead("HKCU\" regPath, valueName)
        
        if (currentValue = "0x00000001")
        {
            RegWrite("0x00000000", "REG_SZ", "HKCU\" regPath, valueName)
            WinActivate("ahk_class Shell_TrayWnd")
            Sleep(50)
            WinActivate("ahk_id " currentWindow)
            if (IsOpenTrayTip)
            {
                ShowOSD("繁體")
            }
        }
        else if (currentValue = "0x00000000")
        {
            RegWrite("0x00000001", "REG_SZ", "HKCU\" regPath, valueName)
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
        MsgBox("錯誤：輸入法註冊表項目不存在`n" err.Message, "錯誤", "Icon!")
    }
    
    return
}
