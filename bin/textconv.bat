REM set P1=C:\resin\resin-pro-4.0.64\webapps\imart\WEB-INF\jssp\src\jae\0010_conbsnplan\pricechg
set P1="C:\tool\textconv\bin\webyml\1800_OfficeSupply_WH.yml"
REM set P1=C:\tmp\wangx\_projects\wang2\src\src122
REM set P1=C:\tmp\wangx\_projects\jae\develop\jae\jae\src\main\jssp\src\jae\0000_common\proposal\
REM cd C:\tool\textconv\bin
set P1="C:\Users\lqwan\OneDrive - �l�a�o�W���p���������\projects\toyotec\02_SRC"

textconv -c title -x xpath -d %P1%\220
REM textconv -c comment -d %P1%
REM textconv -web -f %P1%


REM taskkill /im textconv.exe /F
REM taskkill /im IEDriverServer.exe /F
