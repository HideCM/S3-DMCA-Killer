
## Installation Instructions

1. Download the AWS CLI installer from the following link:  
   [AWS CLI Download](https://awscli.amazonaws.com/AWSCLIV2.msi)

2. After the download is complete, open the Terminal (Windows Command Prompt).

3. Type the following command to check if AWS CLI is installed successfully:
```bash
   aws --version
```
4. If the output shows something like:
```bash
aws-cli/2.19.1 Python/3.11.6 Windows/10 exe/AMD64 prompt/off
```
5. To configure the AWS CLI for the Backblaze profile, run the following commands:

aws configure set profile.**backblaze**.aws_access_key_id **0028aa4dc47*****0002**
aws configure set profile.**backblaze**.aws_secret_access_key **K***************X5JB9g**
aws configure set profile.**backblaze**.region **us-west-002**
aws configure set profile.**backblaze**.output json
aws configure set profile.**backblaze**.s3.endpoint_url **https://s3.us-west-002.backblazeb2.com**

*Change the **backblade** to your desired name, and make sure it has the same name as CMD and *config.ini** profile=**backblade** or something you want

## Sample config.ini
```ini
[Profile_backblaze]
bucketName=my-test-bucket
profile=backblaze
endpointUrl=https://s3.us-west-002.backblazeb2.com
dmcaFilePath=data/dmca-backblaze.txt
doneFilePath=data/dmca-done-backblaze.txt

[Profile_aws]
bucketName=my-aws-bucket
profile=aws
endpointUrl=https://s3.amazonaws.com
dmcaFilePath=data/dmca-aws.txt
doneFilePath=data/dmca-done-aws.txt

[Settings]
defaultProfile=backblaze
```
### How to Add New Configurations

-   To add a new profile, follow the structure shown above. 
-   Make sure each new profile starts with `Profile_` to *config.ini* file

### File Requirements

Ensure that you have created the necessary files as specified in the configuration, such as:

-   `dmca-backblaze.txt` (used for input data for deletion)
-   `dmca-done-backblaze.txt` (used to save deleted files)

### Data Format

The data should be entered in the following format:

    my-test-bucket/user_files/abc/file-918-1-1730042741/0012.png
