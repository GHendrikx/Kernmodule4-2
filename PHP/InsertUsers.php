<?php
include 'DatabaseConnection.php';

//Score and session_ID
SetExistingSession();
session_start();

if (!CheckServerID()) {
    echo "0";
    return;
}

$User_id = $_GET["Username"];
$email = $_GET["Email"];
$Password = $_GET["Password"];

$dupemail = "SELECT Email FROM Users WHERE Email = $email";

if (@mysql_query($dupemail)) 
{
echo("<p>Error: Your email is already in our database.</p>");
exit;
}
else
{
$query = "INSERT INTO Users (id, Username, Email, Password, Datetime) VALUES (NULL, '$User_id', '$email', '$Password' , now())";
}
if (!($result = $mysqli->query($query))){
  showerror($mysqli->errno,$mysqli->error);
}

else {
	echo 1;
  }

?>
