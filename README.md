# Lightning-Pro

The following repo is used to store and version Lightning Pro app as part of Eaton's Initiative to go digital.

## Get Started 

1. To start working in this repo you must first install [git bash](https://git-scm.com/download/win) Note: If this is your first time using git the following [video](https://www.youtube.com/watch?v=USjZcfj8yxE) can make it seem simple
2. Open the git bash terminal 
3. Run the following command to turn off SSL verification `git config --global http.sslverify "false"` 
4. Clone the repo with the following command: `git clone https://github.com/Eaton-Corp/Lightning-Pro.git`

## Start Developing

Alright! Alright! Hold your horses noww! You make be excited to get coding, but you gotta set up some configs first.

1. Before your able to successfully start ensure that you have the following dependencies installed on your computer.
- Microsoft Access 32-bit
- Microsoft Visual Studio 2022 (includes Nuget, C# and WPF)

2. Please open the config located at \Local_Config and change the field to ensure the repository location matches what is your local machines path e.g `{path to repository} = C:\Users\e0637402\Repos` Note: This is the file path for the documenters PC your's will have a different path. 

3. Make sure on boot of lightning Pro you select the config file which is stored in this repository under /Local_Config.

4. You should be good to go from here. Happy Coding!!

# Branching Strategy

Woahh there boi. Before you start adding code willy nilly make sure your branching correctly. 

1. Create your own branch with the following command `git branch {your_branch_name}`

2. Now you must checkout your branch so you can make your changes `git checkout {your_branch_name}`

3. Write your code, commit your changes, then commit to your branch.

4. When your code has been pushed you will see the option "Compare and Pull Request" on the main page. More info can be found on how to set up PR's [here](https://www.freecodecamp.org/news/how-to-make-your-first-pull-request-on-github-3/#:~:text=Create%20pull%20request,Congratulations!)

5. Send your PR to atleast 1 engineer on the team to ensure changes are functional and correct

6. Finally if your changes got approved, LGTM!! Merge the changes into the main branch. 

7. Monitor for issues relating to change.
 
## Deployment
