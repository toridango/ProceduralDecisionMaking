import sys
import numpy as np
import matplotlib.pyplot as plt
import xml.etree.ElementTree as ET


class NPC:

    # <!-- -100 to 100 -->
    # <personality>

    # <!-- 0 to 100 -->
    # <skills>

    # <!-- 0 to 100 -->
    # <pseudoskills>

    # <!-- -100 to 100 -->
    # <allegiances>



    def __init__(self):
        self.attr = {}
    
    
    def loadFromFile(self, filename):
        tree = ET.parse(filename)
        root = tree.getroot()
        self.loadFromXMLTree(root)

    def loadFromXMLTree(self, xmlTree):
        # print(xmlTree.)
        for child in xmlTree.getchildren():
            if child.tag != "dialog":
                self.attr[child.tag] = {}  
                for grandchild in child.getchildren():
                    self.attr[child.tag][grandchild.tag] = float(grandchild.text)

    def printAttributes(self):
        for k in self.attr:
            for sk in self.attr[k]:
                print(k, sk, self.attr[k][sk])


    def rankGetItem(self):

        print("Ranking...")
        utilities = []
        utilities.append(("Steal", stealFunc(
                                    self.attr["personality"]["conscience"], 
                                    perceivedSkill(self.attr["skills"]["pickpocket"], self.attr["personality"]["confidence"]),
                                    perceivedSkill(self.attr["skills"]["stealth"], self.attr["personality"]["confidence"])
                                    )
        ))
        utilities.append(("Buy", buyFunc(
                                    self.attr["pseudoskills"]["wealth"], 
                                    self.attr["personality"]["carelessness"]
                                    )
        ))
        utilities.append(("Persuade", 
                                    persuadeFunc(perceivedSkill(
                                    self.attr["skills"]["charisma"], self.attr["personality"]["confidence"]),
                                    self.attr["personality"]["friendliness"]
                                    )
        ))
        utilities.append(("Intimidate", 
                                    intimidateFunc(perceivedSkill(self.attr["skills"]["combat"], self.attr["personality"]["confidence"]),
                                    self.attr["personality"]["friendliness"]
                                    )
        ))
        utilities.append(("Craft", craftFunc(
                                    perceivedSkill(self.attr["skills"]["crafts"], self.attr["personality"]["confidence"])
                                    )
        ))

        utilities.sort(key = lambda x: x[1])

        for u in utilities:
            print("\t"+u[0], u[1])




def confidenceSlope(confidence):
    
    c = confidence / 100.0
    
    # arbitrary constants to shape the function
    vertDisplacement = 0.25
    expMult = 2.34      # q
    xIncrement = 0.23    # r

    if confidence > -50.0:
        slope = 1 + 2*(c**3)
    else:
        slope = vertDisplacement + 3**(expMult * (c + xIncrement))

    return slope

def plotConfidenceSlope():

    x = np.arange(-100.0, 100.0, 1.0)
    y = np.array([confidenceSlope(xi) for xi in x])
    plt.figure(1)
    plt.xlabel('confidence')
    plt.ylabel('slope for real-to-perceived line')
    plt.title('Slope of skill perception vs confidence')

    plt.plot(x, y)
    ax = plt.axes()
    plt.grid(True)
    ax.axvline(x = 0, color = 'k')
    ax.axhline(y = 0, color = 'k')
    plt.show()



def perceivedSkill(skill, confidence):

    return confidenceSlope(confidence) * skill

def plotPerceivedSkill(confidence):

    x = np.arange(0.0, 100.0, 1.0)
    plt.figure(2)
    plt.xlabel('real skill')
    plt.ylabel('perceived skill')
    plt.title('Perceived vs real skill given confidence: {}'.format(str(confidence)))

    plt.plot(x, perceivedSkill(x, confidence))
    ax = plt.axes()
    plt.grid(True)
    ax.axvline(x = 0, color = 'k')
    ax.axhline(y = 0, color = 'k')
    plt.show()




def stealFunc(conscience, p_pickpocket, p_stealth):
    # arbitrary constants to shape the function
    psModifier = 0.014       # a
    sigSize = 160           # p
    sigHardness = 0.016      # k
    return psModifier * p_stealth * p_pickpocket  +  0.5*sigSize  +  -sigSize / (1 + np.exp(-sigHardness * conscience))

def plotSteal(pickpocket, stealth, confidence):

    x = np.arange(-100.0, 100.0, 1.0)
    plt.figure(3)
    plt.xlabel('Conscience')
    plt.ylabel('Utility')
    plt.title('Steal utility given stealth: {}, pickpocket: {}, confidence: {} '.format(str(stealth), str(pickpocket), str(confidence)))

    plt.plot(x, stealFunc(x, perceivedSkill(pickpocket, confidence), perceivedSkill(stealth, confidence)))
    ax = plt.axes()
    plt.grid(True)
    ax.axvline(x = 0, color = 'k')
    ax.axhline(y = 0, color = 'k')
    plt.show()




def buyFunc(wealth, carelessness):
    multiplier = 0.4
    padding = 1.5
    xMod = 0.015708
    return multiplier * wealth * (padding + np.sin(xMod * carelessness))

def plotBuy(wealth, carelessness):

    x = np.arange(-100.0, 100.0, 1.0)
    plt.figure(4)
    plt.xlabel('Carelessness')
    plt.ylabel('Utility')
    plt.title('Buy utility given wealth: {}'.format(str(wealth)))

    plt.plot(x, buyFunc(x, wealth))
    ax = plt.axes()
    plt.grid(True)
    ax.axvline(x = 0, color = 'k')
    ax.axhline(y = 0, color = 'k')
    plt.show()





def persuadeFunc(charisma, friendliness):
    vertOffset = -100
    sigSize = 200
    xMod = 0.02
    frDivisor = 200
    return vertOffset + sigSize / (1 + np.exp(-xMod*charisma - friendliness/frDivisor))

def plotPersuade(friendliness):

    x = np.arange(0.0, 100.0, 1.0)
    plt.figure(5)
    plt.xlabel('Charisma')
    plt.ylabel('Utility')
    plt.title('Persuade utility given friendliness: {}'.format(str(friendliness)))

    plt.plot(x, persuadeFunc(x, friendliness))
    ax = plt.axes()
    plt.grid(True)
    ax.axvline(x = 0, color = 'k')
    ax.axhline(y = 0, color = 'k')
    plt.show()




def intimidateFunc(combat, friendliness):
    vertOffset = -101
    sigSize = 200
    xMod = 0.02
    frDivisor = 200
    return vertOffset + sigSize / (1 + np.exp(-xMod*combat + friendliness/frDivisor))

def plotIntimidate(friendliness):

    x = np.arange(0.0, 100.0, 1.0)
    plt.figure(6)
    plt.xlabel('Combat')
    plt.ylabel('Utility')
    plt.title('Intimidate utility given friendliness: {}'.format(str(friendliness)))

    plt.plot(x, intimidateFunc(x, friendliness))
    ax = plt.axes()
    plt.grid(True)
    ax.axvline(x = 0, color = 'k')
    ax.axhline(y = 0, color = 'k')
    plt.show()




def craftFunc(craftsmanship):
    xMod = 0.01
    return xMod * (craftsmanship**2)

def plotCraft():

    x = np.arange(0.0, 100.0, 1.0)
    plt.figure()
    plt.xlabel('Craftsmanship')
    plt.ylabel('Utility')
    plt.title('Craft utility')

    plt.plot(x, craftFunc(x))
    ax = plt.axes()
    plt.grid(True)
    ax.axvline(x = 0, color = 'k')
    ax.axhline(y = 0, color = 'k')
    plt.show(7)



def main():
    print(sys.version)


    xml = '''<?xml version="1.0" encoding="utf-8" ?> 
<npc name = "tester">
    <personality>
        <caution>30</caution>
        <carelessness>-30</carelessness>
        <conscience>-10</conscience>
        <friendliness>-10</friendliness>
        <confidence>20</confidence>
    </personality>

    <skills>
        <pickpocket>50</pickpocket>
        <stealth>50</stealth>
        <charisma>30</charisma>
        <crafts>40</crafts>
        <combat>20</combat>
    </skills>

    <pseudoskills>
        <wealth>20</wealth>
        <appeal>20</appeal>
        <fitness>50</fitness>
    </pseudoskills>

    <allegiances>
      <player>100</player>
    </allegiances>
</npc>'''

    xmlTree = ET.fromstring(xml)

    tester = NPC()
    tester.loadFromXMLTree(xmlTree)

    ethan = NPC()
    ethan.loadFromFile("../Assets/NPCs/NPCDB/npc_ethan.xml")

    # plotConfidenceSlope()
    # plotPerceivedSkill(20)



    # plotSteal(tester.attr["skills"]["pickpocket"], 
    #         tester.attr["skills"]["stealth"], 
    #         tester.attr["personality"]["confidence"])
    # plotBuy(tester.attr["pseudoskills"]["wealth"], 
    #         tester.attr["personality"]["carelessness"])
    # plotPersuade(tester.attr["personality"]["friendliness"])
    # plotIntimidate(tester.attr["personality"]["friendliness"])
    # plotCraft()
    print("Ethan --------------------------------")
    ethan.rankGetItem()
    print("Thief --------------------------------")
    tester.rankGetItem()




if __name__ == "__main__":
    main()