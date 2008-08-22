#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

#Exracts IronPython bugs from DevDiv TFS and Codeplex and sends bug status to 
#the IronPython team.

import sys, nt, clr, datetime
import System, System.IO
from datetime import date, timedelta
from System.IO import Path
from System import DateTime
import smtpmailer
sys.path.append(nt.environ['MERLIN_ROOT'] + '\\Test\\Scripts\\Util')
import teamfoundation as tfs
import Microsoft.TeamFoundation.WorkItemTracking.Client
from Microsoft.TeamFoundation.WorkItemTracking.Client import CoreField

class Bug(object):
    def __init__(self, bug):
        self.bug = bug
    def isactive(self):
        return self.bug["State"] == "Active"
    def is_resolved(self):
        return self.bug["State"] in ("Resolved","Fixed")
    def get_created_date(self):
        return self.bug["Created Date"]
    def __getitem__(self, index):
        return self.bug[index]

class CodeplexBug(Bug):
    def is_p1(self):
        return self.bug["Code Studio Rank"] == "High"
    def is_p2(self):
        return self.bug["Code Studio Rank"] == "Medium"
    def link(self):
        return 'http://www.codeplex.com/IronPython/WorkItem/View.aspx?WorkItemId=%d'\
             % self.bug['Id']
    def is_valid_area_path(self):
        ip = self.bug["Iteration Path"]    
        return ip <> 'IronPython\\2.1' and ip <> 'IronPython\\Future'
    def get_priority(self):
        return self.bug['Code Studio Rank']
    def get_created_by(self):
        return self.bug['CodePlex Reported By']

class TFSBug(Bug):
    def is_p1(self):
        return self.bug["Priority"] == 1
    def is_p2(self):
        return self.bug["Priority"] == 2
    def link(self):
        return 'http://vstfdevdiv:8080/WorkItemTracking/WorkItem.aspx?artifactMoniker=%d'\
             % self.bug['Id']
    def is_valid_area_path(self):
        ip = self.bug["Iteration Path"]
        return ip <> 'Dev10\\Growth Opportunities\\IronPython\\2.1' and \
            ip <> 'Dev10\\Growth Opportunities\\IronPython\\V.Next'
    def get_priority(self):
        return self.bug['Priority']
    def get_created_by(self):
        return self.bug['Created By']

def parse_bugs(bugs, at):   
    p1 = p2 = active = resolved = 0
    lastweek_bugs = []
    for bug in bugs:
        if bug.is_valid_area_path():
            if bug.is_p1():
                p1 += 1
            elif bug.is_p2():
                p2 += 1
        if bug.isactive():
            active += 1
        elif bug.is_resolved():
            resolved += 1
        if  bug.get_created_date()>= at.AddDays(-7):
            lastweek_bugs.append(bug)
    return p1, p2, active, resolved, lastweek_bugs


def sign2class(i):
    if i>0:
        return 'pos'
    elif i<0:
        return 'neg'
    else:
        return 'zero'

def build_bug_table(bugs):
    rows = ''
    for i, bug in enumerate(bugs):        
        priority = bug.get_priority()
        if i%2 == 0:
            clazz = 'r'
        else:
            clazz = 'ar'
        rows = rows + '''\
<tr class="%s">
    <td align="center"><a href="%s">%s</a></td>
    <td align="left">%s</td>
    <td align="center">%s</td>
    <td align="center">%s</td>
    <td align="center">%s</td>
    <td align="center">%s</td>
</tr>
''' % (clazz, bug.link(), bug['Id'], bug['Title'], \
    bug.get_created_by(),\
    bug[CoreField.AssignedTo], bug['State'], priority)
    return rows

TFS_URL = 'vstfdevdiv'
TFS_AREA_PATH = 'Dev10\Visual Studio (VS)\Visual Studio Languages\Growth Opportunities\IronPython'
TFS_PROJECT = 'Dev10'
TFS_QUERY = '''\
SELECT [System.Id] FROM WorkItems
WHERE 
    [System.TeamProject] = '%s' AND
    [System.AreaPath] UNDER '%s' AND
    [System.State] <> 'Closed'    
ORDER BY [System.Id]
'''
CODEPLEX_URL = 'https://tfs01.codeplex.com:443'
CODEPLEX_PROJECT = 'IronPython'
CODEPLEX_QUERY = '''\
SELECT [System.Id] FROM WorkItems 
WHERE 
    [System.TeamProject] = '%s' AND 
    [System.State] <> 'Closed'
ORDER BY [System.Id]
'''

#EMAIL_TO = "olegtk@microsoft.com"
EMAIL_TO = "ipyteam@microsoft.com"

week_ago = (date.today() - timedelta(days=7)).strftime('%m/%d/%Y')
tfs_store = tfs.workitem_store(TFS_URL)
tfs_bugs = list(tfs_store.Query(TFS_QUERY % (TFS_PROJECT, TFS_AREA_PATH)))
tfs_bugs = map(TFSBug, tfs_bugs)
tfs_bugs_week_ago = list(tfs_store.Query((TFS_QUERY + " asof '%s'") % \
    (TFS_PROJECT, TFS_AREA_PATH, week_ago)))
tfs_bugs_week_ago = map(TFSBug, tfs_bugs_week_ago)
#ask for codeplex credentials
user_mapping = {
    'olegtk'        :   'olegt_cp',
    'curth'         :   'CurtHagenlocher_cp',
    'dfugate'       :   'dfugate_cp',
    'dinov'         :   'dinov_cp',
    'hpierson'      :   'harrypierson_cp',
    'jimhug'        :   'JimHugunin_cp',
    'jimmysch'      :   'jimmysch_cp',
    'qingye'        :   'QingYe_cp',
    'sborde'        :   'sborde_cp',
    'srivatsn'      :   'srivatsn_cp'
}
if user_mapping.has_key(System.Environment.UserName):
    codeplex_uname = user_mapping[System.Environment.UserName]
else:
    codeplex_uname = raw_input('Enter your codeplex.com user name:')
codeplex_passwd = raw_input('Enter your codeplex.com password:')
codeplex_store = tfs.workitem_store(CODEPLEX_URL, codeplex_uname, codeplex_passwd)
codeplex_bugs = list(codeplex_store.Query(CODEPLEX_QUERY % CODEPLEX_PROJECT))
codeplex_bugs = map(CodeplexBug, codeplex_bugs)
codeplex_bugs_week_ago = list(codeplex_store.Query((CODEPLEX_QUERY + " asof '%s'") % \
    (CODEPLEX_PROJECT, week_ago)))
codeplex_bugs_week_ago = map(CodeplexBug, codeplex_bugs_week_ago)

p1bugs, p2bugs, active, resolved, lastweek = parse_bugs(tfs_bugs + codeplex_bugs, DateTime.Now)
p1bugs_before, p2bugs_before, active_before, resolved_before, lastweek_before = \
    parse_bugs(tfs_bugs_week_ago + codeplex_bugs_week_ago, DateTime.Now.AddDays(-7))

f = open('bugtracker.html')
email = ''.join(f.readlines())
cdate = date.today().strftime('%m/%d/%Y')
p1_diff = p1bugs - p1bugs_before
p2_diff = p2bugs - p2bugs_before
active_diff = active - active_before
resolved_diff = resolved - resolved_before
last_week_diff = len(lastweek) - len(lastweek_before) 
f.close()
t = (
    cdate,
    sign2class(p1_diff), p1bugs, p1_diff, 
    sign2class(p2_diff), p2bugs, p2_diff, 
    sign2class(active_diff), active, active_diff, 
    sign2class(resolved_diff), resolved, resolved_diff, 
    sign2class(last_week_diff), len(lastweek), last_week_diff, 
    build_bug_table(lastweek)
)
email = email % t
try:    
    smtpmailer.send(EMAIL_TO, "IronPython Bugs - %s" % cdate, email)
except System.Exception, e:
    print 'Failed sending email: ', e