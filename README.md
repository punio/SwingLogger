# SwingLogger
ユピテル ゴルフスイングトレーナー GST-7 BLEを使ってスマホでどうにかするやつ


# About
スマホ用アプリの使い勝手が悪いので作ってみた


# 概要
![概要](./docs/Images/overview.png)

- GST-7 BLEが垂れ流すBLEのデータをスマホアプリで受信します
- スマホアプリからクラウド(Azure)にデータを転送します
- スマホアプリには録画機能があってGST-7の情報を重畳して録画します
- クラウドでは全データを保存して統計画像的な感じで表示します

# Sample
[Web Page (http://swingdataviewer.azurewebsites.net/Home/Graph/811889)](http://swingdataviewer.azurewebsites.net/Home/Graph/811889)

[動画 (https://youtu.be/FKR3qnr_Cqs)](https://youtu.be/FKR3qnr_Cqs)


# アプリについて

スマホ側はXamarin.Formsで作っています。iOS/Androidに対応する予定で作っています。ソースはそのうちスマホ側も上げるかも

## スマホアプリ

iOS/Android両方公開しました、お布施しました

<a style="display: inline-block; overflow: hidden; width: 218px; height: 65px; margin: 16px; background-size: contain;" href="https://apps.apple.com/us/app/swing-logger/id1469511261?mt=8"><img src="./docs/Images/appstore.png"/></a>

<a href='https://play.google.com/store/apps/details?id=com.punio.SwingLogger&pcampaignid=MKT-Other-global-all-co-prtnr-py-PartBadge-Mar2515-1'><img alt='Google Play で手に入れよう' src='https://play.google.com/intl/us-en/badges/images/generic/ja_badge_web_generic.png' width="250" height="97" style="vertical-align:baseline;" /></a>



## スマホアプリの使い方
[MobileHelp.md](https://punio.github.io/SwingLogger/MobileHelp.html)


## Webアプリ

上記アプリで収集したデータの確認はWebアプリでおこないます

[Web Page (http://swingdataviewer.azurewebsites.net/)](http://swingdataviewer.azurewebsites.net/)