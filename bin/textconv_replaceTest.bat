REM set P1=C:\resin\resin-pro-4.0.64\webapps\imart\WEB-INF\jssp\src\jae\0010_conbsnplan\pricechg
set P1="C:\tool\textconv\bin\webyml\1800_PurReq_CommMgmt.yml"
REM set P1=C:\tmp\wangx\_projects\jae\develop\jae\jae\src\main\jssp\src\jae\0000_common\proposal\
cd C:\tool\textconv\bin
REM textconv -x xpath -d %P1%
REM textconv -c DATENULL -d %P1%
REM textconv -web -f %P1%
textconv -p "(#HC#)+" -r "====>UPDATE START===>" -input "#HC##HC# ReplaceRuleItem ri = new ReplaceRuleItem(); #HE##HE#"
REM taskkill /im textconv.exe /F
REM taskkill /im IEDriverServer.exe /F
