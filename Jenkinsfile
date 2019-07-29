pipeline {
  agent { label 'webinar' }
  stages {    
    stage('Build') {
      steps {
        sh 'dotnet build'
        sh 'set'
      }
    }
    stage('Link autotests') {
      steps {
        dir ('testit.linker') {
        	sh 'dotnet bin/Debug/netcoreapp2.0/testit.linker.dll ../webinar/bin/Debug/netcoreapp2.0/webinar.dll'
        }
      }
    }
    stage('Get tests') {
      steps {
        dir ('testit.tools') {
        	sh 'dotnet bin/Debug/netcoreapp2.0/testit.tools.dll get ${WORKSPACE}'
        }
      }
    }
    stage('Start run') {
      steps {
        dir ('testit.tools') {
        	sh 'dotnet bin/Debug/netcoreapp2.0/testit.tools.dll start'
        }
      }
    }
    stage('Run autotests') {
      steps {
        dir ('webinar') {
          	run_tests()
        }
      }
    }
    stage('Complete run') {
      steps {
        dir ('testit.tools') {
        	sh 'dotnet bin/Debug/netcoreapp2.0/testit.tools.dll complete'
        }
      }
    }
  }
}

def run_tests() {
  def points = readJSON file: '../points.json'
  if (!points) {
    return
  }
  points.each {
    if (it) {
      try {
        withEnv(["configurationId=${it.configuration}"]) {
      	  sh "dotnet test -a:../testit.datacollector/bin/Debug/netcoreapp2.0 --collect TestITDataCollector --filter FullyQualifiedName=${it.autotest}"
        }
      }
      catch (err) {
      }
    }
  }
}
