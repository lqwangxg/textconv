::cd C:\workspace\textconv\bin

REM set P1=C:\resin\resin-pro-4.0.64\webapps\imart\WEB-INF\jssp\src\jae\0020_conmnfequip\equipinspect\
set P1=C:\workspace\jae\jae\src\main\jssp\src\jae\0012_consalesplan\publish\
set gitworkpath=C:\workspace\jae
set toolpath=C:\workspace\textconv\bin

call:RunAndCommit INSTR "INSTR�Ή�"

call:RunAndCommit TO_DATE "TO_DATE�ϊ�"
call:RunAndCommit TO_CHAR "TO_CHAR�ϊ�"

call:RunAndCommit TO_NUMBER "TO_NUMBER�ϊ�"
call:RunAndCommit ADD_MONTH "ADD_MONTH�ϊ�"
call:RunAndCommit LPAD "LPAD�ϊ�"
call:RunAndCommit SUBSTRING "SUBSTRING�ϊ�"
call:RunAndCommit LENGTH "LENGTH�ϊ�"
call:RunAndCommit NVL "NVL�ϊ�"
call:RunAndCommit ROWID "ROWID�ϊ�"
call:RunAndCommit NULLIF "NULLIF�ϊ�"

call:RunAndCommit SYSDATE "SYSDATE�ϊ�"
call:RunAndCommit ORA_OR "OR�����ϊ�"

::call:RunAndCommit UCASE_TAB "�e�[�u�����啶��"
::call:RunAndCommit UCASE_COL "COL���啶��"
call:RunAndCommit DECODE "DECODE�ϊ�"
call:RunAndCommit LEFTJOIN "LEFTJOIN�ϊ�"

call:RunAndCommit UserTransaction "UserTransaction��Transaction"
call:RunAndCommit usertabcol "���[�U�e�[�u���C��"
call:RunAndCommit insertscript "migration�X�N���v�g�ǉ�"
call:RunAndCommit showtitle "�^�C�g���Ή�"

call:RunAndCommit DATENULL "DATENULL�Ή�"
call:RunAndCommit delcoimart "�R�����g�A�E�g���ꂽIMART�^�O�폜"
call:RunAndCommit PORTAL "���[������URL�A�h���X�C��"
call:RunAndCommit clearlineaddreturn "JS_clearline���\�b�h��return false�ǉ�"

call:RunAndCommit VWRITECHANGE "HTML_�c�\�������w�i�F�C��"

call:RunAndCommit BtnNoDisplay "HTML_��\���{�^���ɂ��󔒍폜"

call:RunAndCommit HTMLescapeXml "escapeXML�ǉ�"

pause


:: %1:command�@�@%2:git comment
:RunAndCommit

echo %1 ########���s�J�n########
cd %toolpath%
textconv -c %1 -d %P1%

echo Git Commit Start!!!
cd %gitworkpath%
git commit -am %2

echo %1 ########���s����########
goto:eof

