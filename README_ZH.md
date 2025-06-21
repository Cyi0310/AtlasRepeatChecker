# AtlasRepeatChecker
#### [README_EN](https://github.com/Cyi0310/AtlasRepeatChecker/blob/main/README.md)

Unity 編輯器工具，透過分析 [圖集(Sprite Atlas)](https://docs.unity3d.com/ScriptReference/U2D.SpriteAtlas.html) 內部的 `圖片(Sprite/Texture)`，並檢測是否有重複的 `圖集` 關聯著相同的 `圖片`。


## 為啥我要做這個
當`多個圖集`關聯`單個圖片`時，會發生以下事件 ([參考至官方文件](https://docs.unity3d.com/Documentation/Manual/sprite/atlas/distribution/resolve-different-sprite-atlas-scenarios.html))
1. 執行階段Unity會隨機載入某個圖集內部的圖片，那可能會導致非預期的結果出現 
2. 每個 `圖集` 都會為該 `圖片` 保留空間，這會導致在 Build 時資源重複包入，造成不必要的空間浪費
3. 在製作的某些時間點下 Console 會跳出 `xxx.Sprite matches more than one built-in atlases ...` 的 Warning log
4. 製作專案的中後期若要頻繁更動 `圖集` 和 `圖片`的關聯時，重複關聯會讓檢查變得麻煩，難以追蹤圖片到底有沒有被多個圖集所關聯

基於以上原因，再加上目前Unity官方沒有提供較方便的工具能檢查這件事，所以我決定做一個ㄌ!


## 介紹
`AtlasRepeatChecker` 幫助 Unity 開發者找出是否有`多個圖集`關聯`單個圖片`，這種情況可能導致不必要的資源重複或增加建置檔案大小。此工具在解析過程中去偵測`圖集`內部`圖片`的 `GUID`並進行精確的檢查，確保`單個圖片`只被`單個圖集`所關聯。


## Demo
> ![](https://github.com/user-attachments/assets/f3bcfd8e-3aa9-4e89-bdbe-7e0d7d7f106f)


## Feature
- 分析專案內 `多個資料夾` 或 `個別的圖集檔案(.spriteatlas)`
- 具備拖放支援的簡易介面，方便使用者操作
- 顯示有被多個圖集關聯的`單個圖片`，並預覽和且能直接導到相對應的路徑
- 簡易的搜尋功能，能直接找到指定的圖片


## 使用方法
1. 透過 (`Tools > AtlasRepeatChecker`) 開啟工具
2. 新增來源至 `Sources` 區塊
   - 拖拉 資料夾/圖集 至 `Sources` 區塊中
   - 使用 "Add Folder" / "Add Atlas" 按鈕，並選取相對應的來源
3. 點擊 "Analyze Atlas Guids"
4. 查看檢查結果，並導航到重複的圖集or圖片


## 安裝方式
### Unity Package Manager（推薦）
1. 開啟 Unity Package Manager (`Window > Package Manager`)
2. 點擊左上角的 `+` 按鈕
3. 選擇 `Add package from git URL...`
4. 輸入以下 URL: `https://github.com/Cyi0310/AtlasRepeatChecker.git`
5. 點擊 `Add`


## 要求
- Unity 2021.3 或更新版本
- 相容於 Windows/Mac/Linux
- 於`編輯模式`下運作，不支援 `執行階段(Play Mode)`


## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.