-- Database: inctrak_errors

-- DROP DATABASE inctrak_errors;

CREATE DATABASE inctrak_errors;

\connect inctrak_errors

CREATE TABLE OPTIONEEPLAN_ERRORS(
	OPTIONEEPLAN_ERROR_PK serial NOT NULL PRIMARY KEY,
	MESSAGE varchar(256) NOT NULL,
	CALL_STACK text NOT NULL,
	UUID varchar(32) NOT NULL,
	USER_FK uuid NULL,
	CREATED timestamp with time zone NOT NULL DEFAULT now(),
	CODE varchar(32) NOT NULL
);
