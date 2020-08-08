<?php

    //database Login
	$db_user = '';
	$db_pass = '';
	$db_host = '';
	$db_name = '';
   
    // $db_user = 'root';
    // $db_pass = '';
	// $db_host = 'localhost';
	// $db_name = 'geoffreyhendrikx';


/* Open a connection */
$mysqli = new mysqli("$db_host","$db_user","$db_pass","$db_name");

/* check connection */
if ($mysqli->connect_errno) {
   echo "Failed to connect to MySQL: (" . $mysqli->connect_errno() . ") " . $mysqli->connect_error() . "<br/>";
   exit();
}

function showerror($error,$errornr) {
die("Error (" . $errornr . ") " . $error);
}

	function getOrDefault($key)
    {
        if(!isset($_GET[$key]) || empty($_GET[$key]))
        {
            return NULL;
        }
        else
        {
            return $_GET[$key];
        }
    }

    function GetURLVariable($urlVar,$minNumber, $maxNumber, $defaultVal = 0) 
	{
    $result = $defaultVal;
    if ( isset($_GET[$urlVar]) || !empty($_GET[$urlVar]))
    {
        $result = $_GET[$urlVar];
    }
    if ($result > $maxNumber && $maxNumber != -1) {
        $offresultset = $maxNumber;
        }
    if ($result < 0 && $minNumber != -1) {
        $result = $minNumber;
    }

    return $result;
	}

function SetExistingSession() {
    if (isset($_GET['session_id'])) { 
        $sid=htmlspecialchars($_GET['session_id']);
        session_id($sid);
    }
}

function CheckServerID() {
    if (isset($_SESSION["server_id"]) && $_SESSION["server_id"]!=0) {
        return true;
    } else {
        return false;
    }
}

?>
