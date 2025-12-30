# Background
S7 plc 없이도, 로컬에서 사용할 수 있는 테스터가 필요했습니다. <br/>
총 4개의 타입을 read/write 하는 기능이 필요합니다.

# How to use
**1. 서버 띄우기** <br/>
   (docker 설치 필요) <br/> <br/>
   1-1. cmd 창에 입력 <br/>
   `docker run -p 8080:8080 -p 8443:443 -p 102:102 --name softplc fbarresi/softplc:latest-linux` <br/><br/>
   1-2. swagger 띄우기 <br/>
   http://localhost:8080/index.html <br/> <br/>
**2. Swagger 에서 사용할 Datablock 을 생성** <br/>
   <img width="1348" height="798" alt="image" src="https://github.com/user-attachments/assets/50c008fb-9f6e-4889-976a-61fbf1d4bae1" />
   <img width="1348" height="798" alt="image" src="https://github.com/user-attachments/assets/67aee86f-88b8-4206-8e52-ea4f2944f6eb" />
    <br/>
   
**3. 테스터 앱 실행** <br/> <br/>
   <img width="1186" height="543" alt="image" src="https://github.com/user-attachments/assets/ecf578ad-11e0-4997-aaff-a555f3f4c497" />
 size 는 word 단위 입니다. <br/>
   - Bool (1 word)
   - Int (1 word)
   - Float (2 word)
   - String (N word)
   <br/><br/>
