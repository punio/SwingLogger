# SwingLogger
ユピテル ゴルフスイングトレーナー GST-7 BLEを使ってスマホでどうにかするやつ


# About
スマホ用アプリの使い勝手が悪いので作ってみた


# 概要
![概要](./Images/overview.png)

- GST-7 BLEが垂れ流すBLEのデータをスマホアプリで受信します
- スマホアプリからクラウド(Azure)にデータを転送します
- スマホアプリには録画機能があってGST-7の情報を重畳して録画します
- クラウドでは全データを保存して統計画像的な感じで表示します

# Sample
[Web Page (http://swingdataviewer.azurewebsites.net/Home/Graph/811889)](http://swingdataviewer.azurewebsites.net/Home/Graph/811889)

[動画 (https://youtu.be/FKR3qnr_Cqs)](https://youtu.be/FKR3qnr_Cqs)


# アプリについて
## 使用フレームワーク
スマホ側はXamarin.Formsで作っています。iOS/Androidに対応する予定で作っています。ソースはそのうちスマホ側も上げるかも

## 公開とか
垂れ流されているデータを勝手に受信している非公式アプリなのでそもそもストアに登録とかどうなのこれ？Yさんに怒られるんじゃない？
一応マルチユーザーを考慮して作っているのでスマホのアプリを入れれば使えます

## iOSアプリについて
~~誰も使わない(であろう)アプリを公開(またはTestFlight)するために99ドルお布施するのもな～～~~ お布施しました

[TestFlightでパブリックベータ公開中(https://testflight.apple.com/join/ozi6EsgV)](https://testflight.apple.com/join/ozi6EsgV)




## Androidアプリについて
Ver.0.1リリースしました。Releaseからapk落とせます

確認はnexus5のみなのでそれ以外の端末での動作は確認できていません

Android端末があればな～～あればな～～

## スマホアプリの使い方
[MobileHelp.md](https://github.com/punio/SwingLogger/blob/master/MobileHelp.md)
